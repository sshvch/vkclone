using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Chat.Services;

namespace Chat.Hubs;

[Authorize]
public class ChatHub : Hub
{
    private readonly ChatService _chat;

    public ChatHub(ChatService chat)
    {
        _chat = chat;
    }

    // Отправить сообщение
    public async Task SendMessage(string toUserId, string text)
    {
        var fromUserId = Context.UserIdentifier;
        if (string.IsNullOrEmpty(fromUserId))
            throw new HubException("Не авторизован");

        try
        {
            var message = await _chat.SendAsync(fromUserId, toUserId, text);

            await Clients.User(toUserId).SendAsync("ReceiveMessage", message);
            await Clients.Caller.SendAsync("ReceiveMessage", message);
        }
        catch (InvalidOperationException ex)
        {
            throw new HubException(ex.Message);
        }
    }
}
