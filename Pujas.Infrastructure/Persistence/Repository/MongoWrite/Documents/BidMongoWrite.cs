using MongoDB.Bson.Serialization.Attributes;

namespace Pujas.Infrastructure.Persistence.Repository.MongoWrite.Documents
{
    public class BidMongoWrite
    {
        [BsonId]
        [BsonElement("id")]
        public required string Id { get; set; }

        [BsonElement("userId")]
        public required string UserId { get; set; }

        [BsonElement("auctionId")]
        public required string AuctionId { get; set; }

        [BsonElement("price")]
        public required decimal Price { get; set; }

        [BsonElement("date")]
        public required DateTime Date { get; set; }

    }
}
