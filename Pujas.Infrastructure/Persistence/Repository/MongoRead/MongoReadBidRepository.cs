using MongoDB.Driver;
using MongoDB.Bson;
using log4net;

using Pujas.Domain.Repositories;

using Pujas.Infrastructure.Configurations;
using Pujas.Infrastructure.Interfaces;
using Pujas.Domain.ValueObjects;
using Pujas.Domain.Aggregates;

namespace Pujas.Infrastructure.Persistence.Repository.MongoRead
{
    public class MongoReadBidRepository : IReadBidRepository
    {
        private readonly IMongoCollection<BsonDocument> _bidCollection;

        public MongoReadBidRepository(MongoReadDbConfig mongoConfig)
        {
            _bidCollection = mongoConfig.db.GetCollection<BsonDocument>("bid_read");
        }

        async public Task CreateBid(BsonDocument bid)
        {
            try
            {
                await _bidCollection.InsertOneAsync(bid);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        async public Task<BsonDocument?> GetBidById(string id)
        {
            try
            {
                var filter = Builders<BsonDocument>.Filter.Eq("_id", id);
                var bidResult = await _bidCollection.Find(filter).FirstOrDefaultAsync();

                return bidResult;
            }
            catch (Exception ex) 
            {
                throw;
            }
        }

        async public Task<List<BsonDocument>> GetAuctionBids(string auctionId)
        {
            try
            {
                var filter = Builders<BsonDocument>.Filter.Eq("auctionId", auctionId);
                var bidResult = await _bidCollection.Find(filter).ToListAsync();

                return bidResult;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        async public Task<BsonDocument?> GetAuctionLastBid(string auctionId)
        {
            try
            {
                var filter = Builders<BsonDocument>.Filter.Eq("auctionId", auctionId);
                var sort = Builders<BsonDocument>.Sort.Descending("date");
                var bidResult = await _bidCollection.Find(filter).Sort(sort).Limit(1).FirstOrDefaultAsync();
                return bidResult;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        async public Task<List<BsonDocument>> GetUserParticipateAuctionsBids(string userId, List<string>? auctionIds, DateTime? from, DateTime? to)
        {
            try
            {
                if (auctionIds == null || auctionIds.Count == 0)
                {
                    if (from.HasValue && to.HasValue)
                    {
                        var filter = Builders<BsonDocument>.Filter.And(
                            Builders<BsonDocument>.Filter.Eq("userId", userId),
                            Builders<BsonDocument>.Filter.Gte("date", from.Value),
                            Builders<BsonDocument>.Filter.Lte("date", to.Value)
                        );
                        var bids = await _bidCollection.Find(filter).ToListAsync();
                        return bids;
                    }
                    else
                    {
                        var filter = Builders<BsonDocument>.Filter.Eq("userId", userId);
                        var bids = await _bidCollection.Find(filter).ToListAsync();
                        return bids;
                    }
                } else
                {
                    if (from.HasValue && to.HasValue)
                    {
                        var filter = Builders<BsonDocument>.Filter.And(
                            Builders<BsonDocument>.Filter.Eq("userId", userId),
                            Builders<BsonDocument>.Filter.In("auctionId", auctionIds),
                            Builders<BsonDocument>.Filter.Gte("date", from.Value),
                            Builders<BsonDocument>.Filter.Lte("date", to.Value)
                        );
                        var bids = await _bidCollection.Find(filter).ToListAsync();
                        return bids;
                    } else
                    {
                        var filter = Builders<BsonDocument>.Filter.And(Builders<BsonDocument>.Filter.Eq("userId", userId),
                            Builders<BsonDocument>.Filter.In("auctionId", auctionIds));
                        var bids = await _bidCollection.Find(filter).ToListAsync();
                        return bids;

                    }
                }
            } catch (Exception ex)
            {
                throw;
            }
        }
    }
}