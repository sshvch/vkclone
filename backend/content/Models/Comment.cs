using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Content.Models;

public class Comment
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

 
    [BsonElement("post_id")]
    public string PostId { get; set; } = string.Empty;

    [BsonElement("author_id")]
    public string AuthorId { get; set; } = string.Empty;

    [BsonElement("text")]
    public string Text { get; set; } = string.Empty;

    [BsonElement("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
