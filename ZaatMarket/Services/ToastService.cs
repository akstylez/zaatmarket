namespace ZaatMarket.Services;

public class ToastMessage
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Message { get; set; } = "";
    public string Type { get; set; } = "info"; // "success", "error", "warning", "info"
    public string Icon => Type switch
    {
        "success" => "✅",
        "error" => "🚨",
        "warning" => "⚠️",
        _ => "ℹ️"
    };
}

public class ToastService
{
    public event Action? OnChange;
    public List<ToastMessage> Messages { get; private set; } = new();

    public void ShowSuccess(string message) => Show(message, "success");
    public void ShowError(string message) => Show(message, "error");
    public void ShowWarning(string message) => Show(message, "warning");
    public void ShowInfo(string message) => Show(message, "info");

    public void Remove(ToastMessage toast)
    {
        if (Messages.Contains(toast))
        {
            Messages.Remove(toast);
            OnChange?.Invoke();
        }
    }

    private async void Show(string message, string type)
    {
        var toast = new ToastMessage { Message = message, Type = type };
        Messages.Add(toast);
        OnChange?.Invoke();

        // Automatically dismiss after 4 seconds
        await Task.Delay(4000);
        Remove(toast);
    }
}