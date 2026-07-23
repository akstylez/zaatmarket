namespace ZaatMarket.Services;

public sealed class PaynowSettings
{
    public string IntegrationId { get; set; } = string.Empty;
    public string IntegrationKey { get; set; } = string.Empty;
    public bool UseTestMode { get; set; }
    public string ReturnUrl { get; set; } = string.Empty;
    public string ResultUrl { get; set; } = string.Empty;
}