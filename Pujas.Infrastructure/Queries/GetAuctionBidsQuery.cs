using MediatR;

using Pujas.Application.DTOs;

namespace Pujas.Infrastructure.Queries
{
    public class GetAuctionBidsQuery : IRequest<List<GetBidDto>>
    {
        public string AuctionId { get; set; }

        public GetAuctionBidsQuery(string auctionId)
        {
            AuctionId = auctionId;
        }
    }
}
