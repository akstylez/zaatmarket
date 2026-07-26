using Microsoft.JSInterop;

namespace ZaatMarket.Services;

public class CurrencyStateService
{
    private readonly IJSRuntime _jsRuntime;

    // Default currency is USD
    public string CurrentCurrency { get; private set; } = "USD";

    // Event triggered whenever the currency changes
    public event Action? OnCurrencyChanged;

    public CurrencyStateService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    /// <summary>
    /// Call this when the app loads to restore the user's saved currency preference.
    /// </summary>
    public async Task InitializeAsync()
    {
        try
        {
            var savedCurrency = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "zaat_currency");
            if (!string.IsNullOrWhiteSpace(savedCurrency))
            {
                CurrentCurrency = savedCurrency;
                NotifyStateChanged();
            }
        }
        catch
        {
            // Silently ignore JS interop errors during server-side pre-rendering
        }
    }

    /// <summary>
    /// Updates the active currency, notifies all subscribed UI components, and caches the choice locally.
    /// </summary>
    public async Task UpdateCurrencyAsync(string newCurrency)
    {
        if (CurrentCurrency != newCurrency)
        {
            CurrentCurrency = newCurrency;
            NotifyStateChanged();

            try
            {
                await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "zaat_currency", newCurrency);
            }
            catch
            {
                // Silently ignore if localStorage is unavailable or restricted
            }
        }
    }

    /// <summary>
    /// Helper: Converts a base USD price to the currently selected currency.
    /// </summary>
    public decimal Convert(decimal amountInUsd)
    {
        return CurrentCurrency switch
        {
            "ZWG" => Math.Round(amountInUsd * 25.0m, 2), // Example ZWG/ZiG rate (adjust as needed)
            "ZAR" => Math.Round(amountInUsd * 18.5m, 2), // Example ZAR rate
            _ => Math.Round(amountInUsd, 2)
        };
    }

    /// <summary>
    /// Helper: Returns a formatted price string with the correct currency symbol (e.g., "$10.00" or "ZiG 250.00").
    /// </summary>
    public string Format(decimal amountInUsd)
    {
        var converted = Convert(amountInUsd);
        return CurrentCurrency switch
        {
            "ZWG" => $"ZiG {converted:N2}",
            "ZAR" => $"R {converted:N2}",
            _ => $"${converted:N2}"
        };
    }

    private void NotifyStateChanged() => OnCurrencyChanged?.Invoke();
}