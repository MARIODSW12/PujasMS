using MongoDB.Bson;

namespace Pujas.Infrastructure.Interfaces
{
    public interface IReadBidRepository
    {
        Task CreateBid(BsonDocument bid);
        Task<BsonDocument?> GetBidById(string id);
        Task<List<BsonDocument>> GetAuctionBids(string auctionId);
        Task<BsonDocument?> GetAuctionLastBid(string auctionId);
        Task<List<BsonDocument>> GetUserParticipateAuctionsBids(string userId, List<string>? auctionIds, DateTime? from, DateTime? to);

    }
}
