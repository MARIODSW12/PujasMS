using MediatR;
using log4net;

using Pujas.Application.DTOs;

using Pujas.Infrastructure.Interfaces;
using MongoDB.Bson;

namespace Pujas.Infrastructure.Queries.QueryHandlers
{
    public class GetAuctionLastBidQueryHandler : IRequestHandler<GetAuctionLastBidQuery, GetBidDto?>
    {
        private readonly IReadBidRepository _bidReadRepository;

        public GetAuctionLastBidQueryHandler(IReadBidRepository bidReadRepository)
        {
            _bidReadRepository = bidReadRepository ?? throw new ArgumentNullException(nameof(bidReadRepository));
        }

        public async Task<GetBidDto?> Handle(GetAuctionLastBidQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var bid = await _bidReadRepository.GetAuctionLastBid(request.AuctionId);

                if (bid == null)
                {
                    return null;
                }

                var bidDto = new GetBidDto
                {
                    Id = bid["_id"].AsString,
                    UserId = bid["userId"].AsString,
                    AuctionId = bid["auctionId"].AsString,
                    Price = bid["price"].AsDecimal,
                    Date = bid["date"].AsBsonDateTime.ToUniversalTime(),
                };

                return bidDto;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
