using System.Collections.Concurrent;

namespace ZaatMarket.Services;

public sealed class PaynowPaymentStore
{
    private readonly ConcurrentDictionary<string, PaynowPaymentRecord> _payments = new();

    public void Save(PaynowPaymentRecord record)
    {
        _payments[record.Reference] = record;
    }

    public PaynowPaymentRecord? Get(string reference)
    {
        _payments.TryGetValue(reference, out var record);
        return record;
    }

    public void UpdateStatus(
        string reference,
        string status,
        string? paynowReference = null,
        string? pollUrl = null,
        decimal? amount = null,
        string? paymentChannel = null,
        string? paymentInstrument = null)
    {
        _payments.AddOrUpdate(
            reference,
            _ => new PaynowPaymentRecord
            {
                Reference = reference,
                Status = status,
                PaynowReference = paynowReference,
                PollUrl = pollUrl,
                Amount = amount ?? 0m,
                PaymentChannel = paymentChannel,
                PaymentInstrument = paymentInstrument
            },
            (_, existing) =>
            {
                existing.Status = status;
                existing.PaynowReference = paynowReference ?? existing.PaynowReference;
                existing.PollUrl = pollUrl ?? existing.PollUrl;
                existing.Amount = amount ?? existing.Amount;
                existing.PaymentChannel = paymentChannel ?? existing.PaymentChannel;
                existing.PaymentInstrument = paymentInstrument ?? existing.PaymentInstrument;
                existing.UpdatedAt = DateTime.UtcNow;
                return existing;
            });
    }
}

public sealed class PaynowPaymentRecord
{
    public string Reference { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Status { get; set; } = "Created";
    public string? PollUrl { get; set; }
    public string? PaynowReference { get; set; }
    public string? PaymentChannel { get; set; }
    public string? PaymentInstrument { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}