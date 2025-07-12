using MongoDB.Bson;

namespace Pujas.Infrastructure.Interfaces
{
    public interface IReadAutomaticBidRepository
    {
        Task CreateAutomaticBid(BsonDocument bid);
        Task<BsonDocument?> GetUserAutomaticBid(string userId, string auctionId);
        Task<List<BsonDocument>> GetAuctionAutomaticBids(string auctionId, decimal limit);

    }
}
