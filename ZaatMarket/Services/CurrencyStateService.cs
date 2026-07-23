using System;
using System.Threading.Tasks;

namespace ZaatMarket.Services;

public class CurrencyStateService
{
    private readonly CurrencyService _currencyService;

    public string TargetCurrency { get; private set; } = "USD";
    public decimal CurrentExchangeRate { get; private set; } = 1.0m;
    public bool IsLoadingRate { get; private set; } = false;

    public event Action? OnCurrencyChanged;
    public CurrencyStateService(CurrencyService currencyService)
    {
        _currencyService = currencyService;
    }

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

            var rate = await _currencyService.ConvertCurrencyAsync("USD", newCurrencyCode, 1.0m);
            CurrentExchangeRate = rate ?? 1.0m;
        }

        IsLoadingRate = false;
        NotifyStateChanged();
    }

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

    private void NotifyStateChanged() => OnCurrencyChanged?.Invoke();
}