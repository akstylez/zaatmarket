using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using ZaatMarket.Components;
using ZaatMarket.Components.Account;
using ZaatMarket.Data;
using ZaatMarket.Services;

var builder = WebApplication.CreateBuilder(args);

// ==========================================
// 1. DATABASE CONFIGURATION
// ==========================================
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// The correct, native way for Blazor Server + EF Core: Register ONLY the factory. 
// This safely handles both background services and component database creation without DI scope clashes.
builder.Services.AddDbContextFactory<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString)
           .ConfigureWarnings(warnings => warnings.Ignore(RelationalEventId.PendingModelChangesWarning)));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// ==========================================
// 2. BLAZOR & SIGNALR (WEB SOCKETS)
// ==========================================
// Added AddHubOptions to allow very large audio/video file uploads (50 MB limit)
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddHubOptions(options =>
    {
        options.MaximumReceiveMessageSize = 50 * 1024 * 1024; // 50 MB
    });

builder.Services.AddHttpContextAccessor();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddAuthorization();

// ==========================================
// 3. APPLICATION & CUSTOM SERVICES
// ==========================================
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

// Currency & Pricing Engines
builder.Services.AddScoped<CurrencyService>();
builder.Services.AddScoped<CurrencyStateService>();

// Real-Time User Presence Tracker
builder.Services.AddSingleton<PresenceService>();
builder.Services.AddScoped<Microsoft.AspNetCore.Components.Server.Circuits.CircuitHandler, UserCircuitHandler>();

// ZAATT AI Live Service
builder.Services.AddHttpClient<ZaattAiService>();

// ==========================================
// 4. EXTERNAL INTEGRATIONS (EMAIL & PAYNOW)
// ==========================================
// SMTP / Email
builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("Smtp"));
builder.Services.AddTransient<IEmailSender<ApplicationUser>, EmailSender>();
builder.Services.AddTransient<EmailSender>();

// Paynow Payment Gateway
builder.Services.Configure<PaynowSettings>(builder.Configuration.GetSection("Paynow"));
builder.Services.AddSingleton<PaynowPaymentStore>();
builder.Services.AddHttpClient<PaynowService>();

// ==========================================
// 5. IDENTITY & AUTHENTICATION
// ==========================================
builder.Services.AddIdentityCore<ApplicationUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.SignIn.RequireConfirmedEmail = false;
    options.Stores.SchemaVersion = IdentitySchemaVersions.Version3;
})
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

var auth = builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = IdentityConstants.ApplicationScheme;
    options.DefaultAuthenticateScheme = IdentityConstants.ApplicationScheme;
    options.DefaultChallengeScheme = IdentityConstants.ApplicationScheme;
    options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
});

auth.AddIdentityCookies();

auth.AddGoogle(options =>
{
    options.ClientId = builder.Configuration["Authentication:Google:ClientId"]
        ?? throw new InvalidOperationException("Google ClientId missing.");

    options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"]
        ?? throw new InvalidOperationException("Google ClientSecret missing.");

    options.CallbackPath = "/signin-google";
});

var app = builder.Build();

// ==========================================
// 6. HTTP REQUEST PIPELINE
// ==========================================
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

// CRITICAL FIX: UseStaticFiles must be called before MapStaticAssets to serve runtime uploaded images!
app.UseStaticFiles();
app.MapStaticAssets();

app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

// ==========================================
// 7. MINIMAL API ENDPOINTS
// ==========================================
// Optional logout helper
app.MapGet("/auth/logout", async (HttpContext http) =>
{
    await http.SignOutAsync(IdentityConstants.ApplicationScheme);
    await http.SignOutAsync(IdentityConstants.ExternalScheme);
    await http.SignOutAsync(IdentityConstants.TwoFactorRememberMeScheme);
    await http.SignOutAsync(IdentityConstants.TwoFactorUserIdScheme);

    return Results.Redirect("/");
});

// Start Paynow payment
app.MapPost("/api/paynow/start", async (
    PaynowService paynowService,
    PaynowStartRequest request) =>
{
    if (string.IsNullOrWhiteSpace(request.Email) ||
        string.IsNullOrWhiteSpace(request.PhoneNumber) ||
        request.Amount <= 0 ||
        string.IsNullOrWhiteSpace(request.ItemName))
    {
        return Results.BadRequest(new { error = "Invalid payment request." });
    }

    var reference = $"ZAAT-{DateTime.UtcNow:yyyyMMddHHmmss}-{Random.Shared.Next(1000, 9999)}";

    var result = await paynowService.StartEcoCashPaymentAsync(
        reference,
        request.Email.Trim(),
        request.PhoneNumber.Trim(),
        request.Amount,
        request.ItemName.Trim());

    return result.Success
        ? Results.Ok(result)
        : Results.BadRequest(result);
});

// Paynow result webhook callback
app.MapPost("/api/paynow/result", async (HttpRequest request, PaynowPaymentStore store) =>
{
    var form = await request.ReadFormAsync();

    var reference = form["reference"].ToString();
    var amountText = form["amount"].ToString();
    var pollUrl = form["pollurl"].ToString();
    var paynowReference = form["paynowreference"].ToString();
    var status = form["status"].ToString();
    var paymentChannel = form["paymentchannel"].ToString();
    var paymentInstrument = form["paymentinstrument"].ToString();

    decimal.TryParse(amountText, out var amount);

    if (!string.IsNullOrWhiteSpace(reference))
    {
        store.UpdateStatus(
            reference,
            status,
            paynowReference,
            pollUrl,
            amount,
            paymentChannel,
            paymentInstrument);
    }

    return Results.Ok();
});

// Check payment status
app.MapGet("/api/paynow/status/{reference}", async (string reference, PaynowService paynowService) =>
{
    var result = await paynowService.CheckStatusAsync(reference);
    return result.Found ? Results.Ok(result) : Results.NotFound(result);
});

// Return URL target
app.MapGet("/payment-return", (HttpRequest request) =>
{
    var reference = request.Query["reference"].ToString();
    var url = string.IsNullOrWhiteSpace(reference)
        ? "/checkout"
        : $"/checkout?reference={Uri.EscapeDataString(reference)}";

    return Results.Redirect(url);
});

// ==========================================
// 8. BLAZOR & IDENTITY COMPONENT MAPPING
// ==========================================
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapAdditionalIdentityEndpoints();

// ==========================================
// 9. AUTOMATIC DATABASE MIGRATIONS ON STARTUP
// ==========================================
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        // Safely resolve via IDbContextFactory to guarantee thread-safe startup migrations
        var factory = services.GetRequiredService<IDbContextFactory<ApplicationDbContext>>();
        using var context = factory.CreateDbContext();
        context.Database.Migrate();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating the database.");
    }
}

app.Run();

// ==========================================
// 10. DATA TRANSFER OBJECTS
// ==========================================
public sealed class PaynowStartRequest
{
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string ItemName { get; set; } = string.Empty;
}