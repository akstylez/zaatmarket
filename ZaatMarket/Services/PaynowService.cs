using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;

namespace ZaatMarket.Services;

public sealed class PaynowService
{
    private const string RemoteTransactionUrl = "https://www.paynow.co.zw/interface/remotetransaction";
    private const string InitiateTransactionUrl = "https://www.paynow.co.zw/interface/initiatetransaction";

    private readonly PaynowSettings _settings;
    private readonly PaynowPaymentStore _store;
    private readonly HttpClient _httpClient;

    public PaynowService(
        IOptions<PaynowSettings> settings,
        PaynowPaymentStore store,
        HttpClient httpClient)
    {
        _settings = settings.Value;
        _store = store;
        _httpClient = httpClient;
    }

    public async Task<PaynowStartResult> StartEcoCashPaymentAsync(
        string reference,
        string customerEmail,
        string customerPhone,
        decimal amount,
        string itemName)
    {
        var formattedPhone = customerPhone?.Trim() ?? "";
        if (formattedPhone.StartsWith("263")) formattedPhone = "0" + formattedPhone.Substring(3);
        else if (formattedPhone.StartsWith("+263")) formattedPhone = "0" + formattedPhone.Substring(4);

        var safeItemName = Regex.Replace(itemName ?? "", @"[^a-zA-Z0-9\s]", "").Trim();
        if (string.IsNullOrWhiteSpace(safeItemName)) safeItemName = "ZaatMarket Order";

        var merchantTrace = CreateMerchantTrace(reference);

        var fields = new List<KeyValuePair<string, string>>
        {
            new("id", _settings.IntegrationId?.Trim() ?? ""),
            new("reference", reference),
            new("amount", amount.ToString("0.00", CultureInfo.InvariantCulture)),
            new("additionalinfo", safeItemName),
            new("returnurl", _settings.ReturnUrl?.Trim() ?? ""),
            new("resulturl", _settings.ResultUrl?.Trim() ?? ""),
            new("authemail", customerEmail?.Trim() ?? ""),
            new("status", "Message"),
            new("method", "ecocash"),
            new("phone", formattedPhone),
            new("merchanttrace", merchantTrace)
        };

        var hash = GenerateHash(fields.Select(x => x.Value));
        fields.Add(new("hash", hash));

        using var request = new HttpRequestMessage(HttpMethod.Post, RemoteTransactionUrl)
        {
            Content = new FormUrlEncodedContent(fields)
        };

        using var response = await _httpClient.SendAsync(request);
        var raw = await response.Content.ReadAsStringAsync();
        var parsed = ParseOrderedResponse(raw);

        if (!ValidateHash(parsed))
        {
            return new PaynowStartResult { Success = false, Error = $"PAYNOW RAW RESPONSE: {raw}" };
        }

        var data = ToDictionary(parsed);

        if (!data.TryGetValue("status", out var responseStatus) ||
            !string.Equals(responseStatus, "ok", StringComparison.OrdinalIgnoreCase))
        {
            return new PaynowStartResult
            {
                Success = false,
                Error = data.TryGetValue("error", out var error) ? error : "Paynow could not start the EcoCash transaction."
            };
        }

        data.TryGetValue("pollurl", out var pollUrl);
        data.TryGetValue("instructions", out var instructions);
        data.TryGetValue("browserurl", out var browserUrl);
        data.TryGetValue("paynowreference", out var paynowReference);

        var record = new PaynowPaymentRecord
        {
            Reference = reference,
            CustomerEmail = customerEmail,
            CustomerPhone = formattedPhone,
            Amount = amount,
            Status = "Sent",
            PollUrl = pollUrl,
            PaynowReference = paynowReference,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _store.Save(record);

        return new PaynowStartResult
        {
            Success = true,
            Reference = reference,
            PollUrl = pollUrl,
            RedirectLink = browserUrl,
            Instructions = instructions
        };
    }

    public async Task<PaynowStartResult> StartWebPaymentAsync(
        string reference,
        string customerEmail,
        decimal amount,
        string itemName)
    {
        var safeItemName = Regex.Replace(itemName ?? "", @"[^a-zA-Z0-9\s]", "").Trim();
        if (string.IsNullOrWhiteSpace(safeItemName)) safeItemName = "ZaatMarket Order";

        var merchantTrace = CreateMerchantTrace(reference);

        var fields = new List<KeyValuePair<string, string>>
        {
            new("id", _settings.IntegrationId?.Trim() ?? ""),
            new("reference", reference),
            new("amount", amount.ToString("0.00", CultureInfo.InvariantCulture)),
            new("additionalinfo", safeItemName),
            new("returnurl", _settings.ReturnUrl?.Trim() ?? ""),
            new("resulturl", _settings.ResultUrl?.Trim() ?? ""),
            new("authemail", customerEmail?.Trim() ?? ""),
            new("status", "Message"),
            new("merchanttrace", merchantTrace)
        };

        var hash = GenerateHash(fields.Select(x => x.Value));
        fields.Add(new("hash", hash));

        using var request = new HttpRequestMessage(HttpMethod.Post, InitiateTransactionUrl)
        {
            Content = new FormUrlEncodedContent(fields)
        };

        using var response = await _httpClient.SendAsync(request);
        var raw = await response.Content.ReadAsStringAsync();
        var parsed = ParseOrderedResponse(raw);

        if (!ValidateHash(parsed))
        {
            return new PaynowStartResult { Success = false, Error = $"PAYNOW RAW RESPONSE: {raw}" };
        }

        var data = ToDictionary(parsed);

        if (!data.TryGetValue("status", out var responseStatus) ||
            !string.Equals(responseStatus, "ok", StringComparison.OrdinalIgnoreCase))
        {
            return new PaynowStartResult
            {
                Success = false,
                Error = data.TryGetValue("error", out var error) ? error : "Paynow could not generate checkout link."
            };
        }

        data.TryGetValue("pollurl", out var pollUrl);
        data.TryGetValue("browserurl", out var browserUrl);

        var record = new PaynowPaymentRecord
        {
            Reference = reference,
            CustomerEmail = customerEmail,
            CustomerPhone = "N/A",
            Amount = amount,
            Status = "Sent",
            PollUrl = pollUrl,
            PaynowReference = "Pending",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _store.Save(record);

        return new PaynowStartResult
        {
            Success = true,
            Reference = reference,
            PollUrl = pollUrl,
            RedirectLink = browserUrl
        };
    }

    public async Task<PaynowStatusResult> CheckStatusAsync(string reference)
    {
        var record = _store.Get(reference);
        if (record is null) return new PaynowStatusResult { Found = false, Status = "Unknown" };

        if (string.IsNullOrWhiteSpace(record.PollUrl))
            return new PaynowStatusResult { Found = true, Status = record.Status, Paid = IsPaidStatus(record.Status) };

        using var request = new HttpRequestMessage(HttpMethod.Post, record.PollUrl)
        {
            Content = new FormUrlEncodedContent(Array.Empty<KeyValuePair<string, string>>())
        };

        using var response = await _httpClient.SendAsync(request);
        var raw = await response.Content.ReadAsStringAsync();
        var parsed = ParseOrderedResponse(raw);

        if (!ValidateHash(parsed)) return new PaynowStatusResult { Found = true, Status = "InvalidHash", Paid = false };

        var data = ToDictionary(parsed);
        data.TryGetValue("status", out var status);
        data.TryGetValue("paynowreference", out var paynowReference);
        data.TryGetValue("pollurl", out var pollUrl);
        data.TryGetValue("paymentchannel", out var paymentChannel);
        data.TryGetValue("paymentinstrument", out var paymentInstrument);

        decimal amount = record.Amount;
        if (data.TryGetValue("amount", out var amountText))
            decimal.TryParse(amountText, NumberStyles.Any, CultureInfo.InvariantCulture, out amount);

        var latestStatus = status ?? record.Status;

        _store.UpdateStatus(reference, latestStatus, paynowReference, pollUrl, amount, paymentChannel, paymentInstrument);

        return new PaynowStatusResult { Found = true, Status = latestStatus, Paid = IsPaidStatus(latestStatus) };
    }

    private string GenerateHash(IEnumerable<string> values)
    {
        var key = _settings.IntegrationKey?.Trim() ?? string.Empty;
        var concat = string.Concat(values) + key;
        var bytes = SHA512.HashData(Encoding.UTF8.GetBytes(concat));
        return Convert.ToHexString(bytes).ToUpperInvariant();
    }

    private bool ValidateHash(List<KeyValuePair<string, string>> fields)
    {
        var incomingHash = fields.FirstOrDefault(x => string.Equals(x.Key, "hash", StringComparison.OrdinalIgnoreCase)).Value;
        if (string.IsNullOrWhiteSpace(incomingHash)) return false;

        var values = fields.Where(x => !string.Equals(x.Key, "hash", StringComparison.OrdinalIgnoreCase)).Select(x => x.Value);
        var expected = GenerateHash(values);
        return string.Equals(expected, incomingHash, StringComparison.OrdinalIgnoreCase);
    }

    // THE FIX IS HERE: Replacing the + signs with spaces before unescaping
    private static List<KeyValuePair<string, string>> ParseOrderedResponse(string raw)
    {
        var result = new List<KeyValuePair<string, string>>();
        if (string.IsNullOrWhiteSpace(raw)) return result;

        var pairs = raw.Split('&', StringSplitOptions.RemoveEmptyEntries);
        foreach (var pair in pairs)
        {
            var parts = pair.Split('=', 2);

            // Replace '+' with a space BEFORE decoding
            var key = Uri.UnescapeDataString(parts[0].Replace("+", " "));
            var value = parts.Length > 1 ? Uri.UnescapeDataString(parts[1].Replace("+", " ")) : string.Empty;

            result.Add(new KeyValuePair<string, string>(key, value));
        }
        return result;
    }

    private static Dictionary<string, string> ToDictionary(IEnumerable<KeyValuePair<string, string>> fields)
    {
        return fields.ToDictionary(x => x.Key, x => x.Value, StringComparer.OrdinalIgnoreCase);
    }

    private static bool IsPaidStatus(string? status)
    {
        return string.Equals(status, "Paid", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(status, "Awaiting Delivery", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(status, "Delivered", StringComparison.OrdinalIgnoreCase);
    }

    private static string CreateMerchantTrace(string reference)
    {
        var cleaned = new string(reference.Where(char.IsLetterOrDigit).ToArray());
        if (cleaned.Length > 24) cleaned = cleaned[..24];
        return $"{cleaned}{Random.Shared.Next(1000, 9999)}";
    }
}

public sealed class PaynowStartResult
{
    public bool Success { get; set; }
    public string? Reference { get; set; }
    public string? PollUrl { get; set; }
    public string? RedirectLink { get; set; }
    public string? Instructions { get; set; }
    public string? Error { get; set; }
}

public sealed class PaynowStatusResult
{
    public bool Found { get; set; }
    public bool Paid { get; set; }
    public string Status { get; set; } = "Unknown";
}