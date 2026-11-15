using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Chat.Models;

public class ChatMessage
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    [BsonElement("from_user_id")]
    public string FromUserId { get; set; } = string.Empty;

    [BsonElement("to_user_id")]
    public string ToUserId { get; set; } = string.Empty;

    [BsonElement("text")]
    public string Text { get; set; } = string.Empty;

    [BsonElement("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
