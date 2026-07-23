using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.Circuits;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using ZaatMarket.Data;

namespace ZaatMarket.Services;

public class UserCircuitHandler : CircuitHandler
{
    private readonly AuthenticationStateProvider _authStateProvider;
    private readonly PresenceService _presenceService;
    private readonly IServiceScopeFactory _scopeFactory;

    public UserCircuitHandler(
        AuthenticationStateProvider authStateProvider,
        PresenceService presenceService,
        IServiceScopeFactory scopeFactory)
    {
        _authStateProvider = authStateProvider;
        _presenceService = presenceService;
        _scopeFactory = scopeFactory;
    }

    public override async Task OnConnectionUpAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        var authState = await _authStateProvider.GetAuthenticationStateAsync();
        var userId = authState.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!string.IsNullOrEmpty(userId))
        {
            _presenceService.UserConnected(userId);
        }
    }

    public override async Task OnConnectionDownAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        var authState = await _authStateProvider.GetAuthenticationStateAsync();
        var userId = authState.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!string.IsNullOrEmpty(userId))
        {
            _presenceService.UserDisconnected(userId);

            // Save their "Last Seen" time to the database
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var user = await db.Users.FindAsync(userId);
            if (user != null)
            {
                user.LastSeenUtc = DateTime.UtcNow;
                await db.SaveChangesAsync(cancellationToken);
            }
        }
    }
}