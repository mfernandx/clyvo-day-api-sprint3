using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ClyvoDayApiWeb.MongoDB.Documents
{
    public class PetTimelineEntry
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("petId")]
        public int PetId { get; set; }

        [BsonElement("createdByUserId")]
        public int CreatedByUserId { get; set; }

        [BsonElement("title")]
        public string Title { get; set; } = string.Empty;

        [BsonElement("category")]
        public string Category { get; set; } = string.Empty;

        [BsonElement("description")]
        public string Description { get; set; } = string.Empty;

        [BsonElement("registeredAt")]
        public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;

        [BsonElement("tags")]
        public List<string> Tags { get; set; } = new();

        [BsonElement("details")]
        public Dictionary<string, string> Details { get; set; } = new();
    }
}
