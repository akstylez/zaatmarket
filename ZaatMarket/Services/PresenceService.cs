using System.Collections.Concurrent;

namespace ZaatMarket.Services;

public class PresenceService
{
    // Tracks UserId -> Number of open tabs/devices
    private readonly ConcurrentDictionary<string, int> _onlineUsers = new();

    // Event we can trigger so the UI updates instantly
    public event Action? OnPresenceChanged;

    public void UserConnected(string userId)
    {
        _onlineUsers.AddOrUpdate(userId, 1, (_, count) => count + 1);
        OnPresenceChanged?.Invoke();
    }

    public void UserDisconnected(string userId)
    {
        if (_onlineUsers.TryGetValue(userId, out int count))
        {
            if (count <= 1)
            {
                _onlineUsers.TryRemove(userId, out _);
            }
            else
            {
                _onlineUsers[userId] = count - 1;
            }
            OnPresenceChanged?.Invoke();
        }
    }

    public bool IsOnline(string userId)
    {
        return _onlineUsers.ContainsKey(userId);
    }
}