using MassTransit;
using MongoDB.Bson;
using log4net;

using Pujas.Domain.Events;

using Pujas.Infrastructure.Interfaces;
using Pujas.Domain.Aggregates;

namespace Pujas.Infrastructure.Consumer
{
    public class CreateBidConsumer(IServiceProvider serviceProvider, IReadBidRepository bidReadRepository) : IConsumer<BidCreatedEvent>
    {
        private readonly IServiceProvider _serviceProvider = serviceProvider;
        private readonly IReadBidRepository _bidReadRepository = bidReadRepository;

        public async Task Consume(ConsumeContext<BidCreatedEvent> @event)
        {

            try
            {
                var message = @event.Message;
                var bsonBid = new BsonDocument
                {
                    { "_id",  message.BidId},
                    {"userId", message.UserId},
                    {"price", message.Price},
                    {"date", message.Date},
                    { "auctionId", message.AuctionId},
                };

                await _bidReadRepository.CreateBid(bsonBid);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}