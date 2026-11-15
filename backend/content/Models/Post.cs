using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Content.Models;

public class Post
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    [BsonElement("author_id")]
    public string AuthorId { get; set; } = string.Empty;

    [BsonElement("text")]
    public string Text { get; set; } = string.Empty;

    [BsonElement("image_url")]
    public string? ImageUrl { get; set; }

    [BsonElement("likes")]
    public List<string> Likes { get; set; } = new();

    [BsonElement("likes_count")]
    public int LikesCount { get; set; } = 0;

    [BsonElement("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
