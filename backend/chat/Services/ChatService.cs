using Chat.DTOs;
using Chat.Models;
using Chat.Repositories;

namespace Chat.Services;

public class ChatService
{
    private const int MaxMessageLength = 2000;

    private readonly MessageRepository _messages;

    public ChatService(MessageRepository messages)
    {
        _messages = messages;
    }

    public async Task<DtoMessage> SendAsync(string fromUserId, string toUserId, string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            throw new InvalidOperationException("Сообщение пустое");

        if (text.Length > MaxMessageLength)
            throw new InvalidOperationException($"Сообщение длиннее {MaxMessageLength} символов");

        if (fromUserId == toUserId)
            throw new InvalidOperationException("Нельзя писать самому себе");

        var message = new ChatMessage
        {
            FromUserId = fromUserId,
            ToUserId = toUserId,
            Text = text.Trim()
        };

        await _messages.AddAsync(message);

        return new DtoMessage(message);
    }

    public async Task<List<DtoMessage>> GetHistoryAsync(string userId, string peerId, int limit = 100)
    {
        var messages = await _messages.GetConversationAsync(userId, peerId, limit);
        return messages.Select(m => new DtoMessage(m)).ToList();
    }
}
