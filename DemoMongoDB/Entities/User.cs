using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace DemoMongoDB.Entities
{
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
        public string UserName { get; set; } = null!;
        public string Password { get; set; } = null!;

    }
}
