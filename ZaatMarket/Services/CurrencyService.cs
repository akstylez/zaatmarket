using System.Net.Http.Json;
using ZaatMarket.Models;

namespace ZaatMarket.Services;

public class CurrencyService
{
    private readonly HttpClient _httpClient;
    // this is the api KEY
    private readonly string _apiKey = "e284e54b530e28ba99f227008c557f48";

    public CurrencyService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<decimal?> ConvertCurrencyAsync(string fromCurrency, string toCurrency, decimal amount)
    {
        try
        {
            var url = $"https://api.exchangerate.host/convert?access_key={_apiKey}&from={fromCurrency}&to={toCurrency}&amount={amount}";

            var response = await _httpClient.GetFromJsonAsync<ExchangeRateResponse>(url);

            if (response != null && response.Success)
            {
                return response.Result;
            }

            return null;
        }
        catch
        {
            // If the API is down or there is a network error it returns 0
            return null;
        }
    }
}