using MediatR;
using log4net;

using Pujas.Application.DTOs;

using Pujas.Infrastructure.Interfaces;
using MongoDB.Bson;

namespace Pujas.Infrastructure.Queries.QueryHandlers
{
    public class GetUserParticipateAuctionsQueryHandler : IRequestHandler<GetUserParticipateAuctionsQuery, List<GetBidDto>>
    {
        private readonly IReadBidRepository _bidReadRepository;

        public GetUserParticipateAuctionsQueryHandler(IReadBidRepository bidReadRepository)
        {
            _bidReadRepository = bidReadRepository ?? throw new ArgumentNullException(nameof(bidReadRepository));
        }

        public async Task<List<GetBidDto>> Handle(GetUserParticipateAuctionsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var bids = await _bidReadRepository.GetUserParticipateAuctionsBids(request.UserId, request.AuctionIds, request.From, request.To);

                var resultBids = new List<GetBidDto>();
                foreach (var bid in bids) { 
                
                    var bidDto = new GetBidDto
                    {
                        Id = bid["_id"].AsString,
                        UserId = bid["userId"].AsString,
                        AuctionId = bid["auctionId"].AsString,
                        Price = bid["price"].AsDecimal,
                        Date = bid["date"].AsBsonDateTime.ToUniversalTime(),
                    };
                    resultBids.Add(bidDto);
                }

                return resultBids;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
