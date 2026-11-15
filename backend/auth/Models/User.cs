using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace auth.Models;

public class User
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    [BsonElement("email")]
    public string Email { get; set; } = string.Empty;

    [BsonElement("password_hash")]
    public string PasswordHash { get; set; } = string.Empty;

    [BsonElement("name")]
    public string FirstName { get; set; } = string.Empty;

    [BsonElement("surname")]
    public string LastName { get; set; } = string.Empty;

    [BsonElement("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
