using MongoDB.Driver;
using MongoDB.Bson;

using Pujas.Infrastructure.Configurations;
using Pujas.Infrastructure.Interfaces;

namespace Pujas.Infrastructure.Persistence.Repository.MongoRead
{
    public class MongoAutomaticReadBidRepository : IReadAutomaticBidRepository
    {
        private readonly IMongoCollection<BsonDocument> _automaticBidCollection;

        public MongoAutomaticReadBidRepository(MongoReadDbConfig mongoConfig)
        {
            _automaticBidCollection = mongoConfig.db.GetCollection<BsonDocument>("automatic_bid_read");
        }

        async public Task CreateAutomaticBid(BsonDocument bid)
        {
            try
            {
                await _automaticBidCollection.InsertOneAsync(bid);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        async public Task<BsonDocument?> GetUserAutomaticBid(string userId, string auctionId)
        {
            try
            {
                var filter = Builders<BsonDocument>.Filter.And(Builders<BsonDocument>.Filter.Eq("auctionId", auctionId), Builders<BsonDocument>.Filter.Eq("userId", userId));


                var result = await _automaticBidCollection.Find(filter).FirstOrDefaultAsync();

                return result;

            }
            catch (Exception ex)
            {
                throw;
            }
        }

        async public Task<List<BsonDocument>> GetAuctionAutomaticBids(string auctionId, decimal limit)
        {
            try
            {
                var filter = Builders<BsonDocument>.Filter.And(Builders<BsonDocument>.Filter.Eq("auctionId", auctionId), Builders<BsonDocument>.Filter.Gte("limit", limit));
                var order = Builders<BsonDocument>.Sort.Ascending("minimum");
                var bidResult = await _automaticBidCollection.Find(filter).Sort(order).ToListAsync();

                return bidResult;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}