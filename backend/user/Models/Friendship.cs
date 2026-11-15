using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace user.Models;

public class Friendship
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    [BsonElement("user_a")]
    public string UserA { get; set; } = string.Empty;

    [BsonElement("user_b")]
    public string UserB { get; set; } = string.Empty;

    [BsonElement("status")]
    public string Status { get; set; } = FriendshipStatus.Pending;

    [BsonElement("requested_by")]
    public string RequestedBy { get; set; } = string.Empty;

    [BsonElement("is_read")]
    public bool IsRead { get; set; }

    [BsonElement("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public static class FriendshipStatus
{
    public const string Pending = "pending";
    public const string Accepted = "accepted";
    public const string Declined = "declined";
}
