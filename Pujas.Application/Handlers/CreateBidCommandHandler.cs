using MediatR;

using Pujas.Application.Commands;

using Pujas.Domain.Repositories;
using Pujas.Domain.ValueObjects;
using Pujas.Domain.Events;
using Pujas.Domain.Factories;
using MassTransit;

namespace Pujas.Application.Handlers
{
    public class CreateBidCommandHandler : IRequestHandler<CreateBidCommand, string>
    {
        private readonly IBidRepository _bidRepository;
        private readonly IPublishEndpoint _publishEndpoint;

        public CreateBidCommandHandler(IBidRepository bidRepository, IPublishEndpoint publishEndpoint)
        {
            _bidRepository = bidRepository ?? throw new ArgumentNullException(nameof(bidRepository));
            _publishEndpoint = publishEndpoint ?? throw new ArgumentNullException(nameof(_publishEndpoint));
        }

        public async Task<string> Handle(CreateBidCommand request, CancellationToken cancellationToken)
        {

            try
            {
                var bidId = Guid.NewGuid().ToString();
                var bid = BidFactory.Create(
                    new VOId(bidId),
                    new VOUserId(request.BidDto.UserId),
                    new VOPrice(request.BidDto.Price),
                    new VODate(request.BidDto.Date),
                    new VOAuctionId(request.BidDto.AuctionId)
                    );

                await _bidRepository.CreateBid(bid);

                var bidCreatedEvent = new BidCreatedEvent(
                    bid.Id.Value, bid.UserId.Value, bid.Price.Value,bid.Date.Value, bid.AuctionId.Value
                );
                await _publishEndpoint.Publish(bidCreatedEvent);

                return bid.Id.Value;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}