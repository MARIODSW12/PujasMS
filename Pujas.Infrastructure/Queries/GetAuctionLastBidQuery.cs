using MediatR;

using Pujas.Application.DTOs;

namespace Pujas.Infrastructure.Queries
{
    public class GetAuctionLastBidQuery : IRequest<GetBidDto?>
    {
        public string AuctionId { get; set; }

        public GetAuctionLastBidQuery(string auctionId)
        {
            AuctionId = auctionId;
        }
    }
}
