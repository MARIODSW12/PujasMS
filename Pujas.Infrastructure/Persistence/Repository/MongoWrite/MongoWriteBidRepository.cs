using MongoDB.Bson;
using MongoDB.Driver;
using log4net;

using Pujas.Domain.Aggregates;
using Pujas.Domain.Repositories;
using Pujas.Domain.Factories;
using Pujas.Domain.ValueObjects;

using Pujas.Infrastructure.Configurations;
using MongoDB.Bson.Serialization.Attributes;

namespace Pujas.Infrastructure.Persistence.Repository.MongoWrite
{
    public class MongoWriteBidRepository : IBidRepository
    {
        private readonly IMongoCollection<BsonDocument> _bidCollection;
        private static readonly ILog _logger = LogManager.GetLogger(typeof(MongoWriteBidRepository));

        public MongoWriteBidRepository(MongoWriteDbConfig mongoConfig)
        {
            _bidCollection = mongoConfig.db.GetCollection<BsonDocument>("bid_write");

        }

        async public Task CreateBid(Bid bid)
        {
            try
            {
                var bsonBid = new BsonDocument
                {
                    { "_id",  bid.Id.Value},
                    {"userId", bid.UserId.Value},
                    {"auctionId", bid.AuctionId.Value},
                    {"price", bid.Price.Value},
                    {"date", bid.Date.Value}
                };

                await _bidCollection.InsertOneAsync(bsonBid);

            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                throw;
            }
        }

        async public Task<Bid?> GetBidById(string id)
        {
            try
            {
                var filter = Builders<BsonDocument>.Filter.Eq("_id", id);
                var bidResult = await _bidCollection.Find(filter).FirstOrDefaultAsync();

                if (bidResult == null)
                    return null;

                var bid = BidFactory.Create(new VOId(bidResult["_id"].AsString), new VOUserId(bidResult["userId"].AsString),
                    new VOPrice(bidResult["price"].AsDecimal), new VODate(bidResult["sate"].AsBsonDateTime.ToUniversalTime()),
                    new VOAuctionId(bidResult["auctionId"].AsString));

                return bid;
            }
            catch (Exception ex) 
            { 
                return null;
            }
        }

    }
}