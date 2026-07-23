using ZaatMarket.Models;

namespace ZaatMarket.Services
{
    public interface IMessageService
    {
        // uses an event to update the ui when a new message arrives
        event Action? OnMessageReceived;

        // get the list of people the user has messaged to
        Task<List<string>> GetConversationPartnersAsync(string currentUserId);

        // gets the actual chat bubbles for a conversation you would have chosen
        Task<List<Message>> GetConversationAsync(string currentUserId, string otherUserId);

        // send a message and triggers the offline email
        Task SendMessageAsync(string senderId, string receiverId, string content, int? productId = null);

        // mark messages as read when the user opens the chat
        Task MarkAsReadAsync(string currentUserId, string otherUserId);
    }
}