using System;
using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace ZaatMarket.Services;

public class CurrencyStateService
{
    private readonly CurrencyService _currencyService;
    private readonly IJSRuntime _jsRuntime;

    // Original properties expected by ProductDetails.razor, Products.razor, and NavMenu.razor
    public string TargetCurrency { get; private set; } = "USD";
    public decimal CurrentExchangeRate { get; private set; } = 1.0m;
    public bool IsLoadingRate { get; private set; } = false;

    // Backward-compatible alias expected by MainLayout.razor
    public string CurrentCurrency => TargetCurrency;

    public event Action? OnCurrencyChanged;

    public CurrencyStateService(CurrencyService currencyService, IJSRuntime jsRuntime)
    {
        _currencyService = currencyService;
        _jsRuntime = jsRuntime;
    }

    /// <summary>
    /// Restores saved currency preference from browser localStorage on initial load.
    /// </summary>
    public async Task InitializeAsync()
    {
        try
        {
            var savedCurrency = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "zaat_currency");
            if (!string.IsNullOrWhiteSpace(savedCurrency) && savedCurrency != TargetCurrency)
            {
                await UpdateCurrencyAsync(savedCurrency);
            }
        }
        catch
        {
            // Silently ignore JS interop errors during server-side pre-rendering
        }
    }

    /// <summary>
    /// Fetches live exchange rates via CurrencyService and caches preference locally.
    /// </summary>
    public async Task UpdateCurrencyAsync(string newCurrencyCode)
    {
        if (TargetCurrency == newCurrencyCode) return;

        IsLoadingRate = true;
        NotifyStateChanged();

        TargetCurrency = newCurrencyCode;

        if (newCurrencyCode == "USD")
        {
            CurrentExchangeRate = 1.0m;
        }
        else
        {
            try
            {
                var rate = await _currencyService.ConvertCurrencyAsync("USD", newCurrencyCode, 1.0m);
                CurrentExchangeRate = rate ?? 1.0m;
            }
            catch
            {
                CurrentExchangeRate = 1.0m; // Fallback to base rate if external API fails
            }
        }

        IsLoadingRate = false;
        NotifyStateChanged();

        try
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "zaat_currency", newCurrencyCode);
        }
        catch
        {
            // Silently ignore if localStorage is unavailable
        }
    }

    // Original method expected by ProductDetails.razor and Products.razor
    public string FormatPrice(decimal baseUsdAmount)
    {
        decimal convertedAmount = baseUsdAmount * CurrentExchangeRate;

        return TargetCurrency switch
        {
            "ZAR" => $"R {convertedAmount:N2}",
            "ZWG" => $"ZiG {convertedAmount:N2}",
            "EUR" => $"€ {convertedAmount:N2}",
            "GBP" => $"£ {convertedAmount:N2}",
            "BWP" => $"P {convertedAmount:N2}",
            _ => $"${convertedAmount:N2}"
        };
    }

    // Backward-compatible alias for MainLayout.razor
    public string Format(decimal baseUsdAmount) => FormatPrice(baseUsdAmount);

    public decimal Convert(decimal amountInUsd) => Math.Round(amountInUsd * CurrentExchangeRate, 2);

    private void NotifyStateChanged() => OnCurrencyChanged?.Invoke();
}