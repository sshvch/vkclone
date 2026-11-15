using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace user.Models;

[BsonIgnoreExtraElements]
public class User
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [BsonElement("email")]
    public string Email { get; set; } = string.Empty;

    [BsonElement("name")]
    public string FirstName { get; set; } = string.Empty;

    [BsonElement("surname")]
    public string LastName { get; set; } = string.Empty;

    [BsonElement("city")]
    public string? City { get; set; }

    [BsonElement("phone")]
    public string? Phone { get; set; }

    [BsonElement("avatar_url")]
    public string? AvatarUrl { get; set; }

    [BsonElement("status")]
    public string? Status { get; set; }

    [BsonElement("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("is_deleted")]
    public bool IsDeleted { get; set; }
}
