using MassTransit;
using MongoDB.Bson;

using Pujas.Application.Events;

using Pujas.Infrastructure.Interfaces;

namespace Pujas.Infrastructure.Consumer
{
    public class CreateAutomaticBidConsumer(IServiceProvider serviceProvider, IReadAutomaticBidRepository bidReadRepository) : IConsumer<AutomaticBidCreatedEvent>
    {
        private readonly IServiceProvider _serviceProvider = serviceProvider;
        private readonly IReadAutomaticBidRepository _bidReadRepository = bidReadRepository;

        public async Task Consume(ConsumeContext<AutomaticBidCreatedEvent> @event)
        {

            try
            {
                var message = @event.Message;
                var Id = Guid.NewGuid().ToString();
                var bsonBid = new BsonDocument
                {
                    { "_id",  Id},
                    {"userId", message.UserId},
                    {"limit", message.Limit},
                    {"date", message.Date},
                    { "auctionId", message.AuctionId},
                    { "minimum", message.Minimum},
                    { "increase", message.Increase},
                };

                await _bidReadRepository.CreateAutomaticBid(bsonBid);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}