using MediatR;
using log4net;

using Pujas.Application.DTOs;

using Pujas.Infrastructure.Interfaces;
using MongoDB.Bson;

namespace Pujas.Infrastructure.Queries.QueryHandlers
{
    public class GetUserAutomaticBidQueryHandler : IRequestHandler<GetUserAutomaticBidQuery, GetAutomaticBidDto?>
    {
        private readonly IReadAutomaticBidRepository _bidReadRepository;

        public GetUserAutomaticBidQueryHandler(IReadAutomaticBidRepository bidReadRepository)
        {
            _bidReadRepository = bidReadRepository ?? throw new ArgumentNullException(nameof(bidReadRepository));
        }

        public async Task<GetAutomaticBidDto?> Handle(GetUserAutomaticBidQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var bid = await _bidReadRepository.GetUserAutomaticBid(request.UserId, request.AuctionId);

                if (bid == null)
                {
                    return null;
                }

                var bidDto = new GetAutomaticBidDto
                {
                    Id = bid["_id"].AsString,
                    UserId = bid["userId"].AsString,
                    AuctionId = bid["auctionId"].AsString,
                    Limit = bid["limit"].AsDecimal,
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
