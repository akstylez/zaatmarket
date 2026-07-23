namespace ZaatMarket.Models;

public class ExchangeRateResponse
{
    public bool Success { get; set; }
    public QueryData? Query { get; set; }
    public decimal Result { get; set; }
}

public class QueryData
{
    public string From { get; set; } = "";
    public string To { get; set; } = "";
    public decimal Amount { get; set; }
}