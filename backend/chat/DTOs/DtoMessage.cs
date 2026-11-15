using Chat.Models;

namespace Chat.DTOs;

public class DtoMessage
{
    public DtoMessage() { }

    public DtoMessage(ChatMessage message)
    {
        Id = message.Id;
        FromUserId = message.FromUserId;
        ToUserId = message.ToUserId;
        Text = message.Text;
        CreatedAt = message.CreatedAt;
    }

    public string Id { get; set; } = string.Empty;
    public string FromUserId { get; set; } = string.Empty;
    public string ToUserId { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
