using MediatR;

using Pujas.Application.DTOs;

namespace Pujas.Infrastructure.Queries
{
    public class GetUserAutomaticBidQuery : IRequest<GetAutomaticBidDto?>
    {
        public string UserId { get; set; }
        public string AuctionId { get; set; }

        public GetUserAutomaticBidQuery(string userId, string auctionId)
        {
            UserId = userId;
            AuctionId = auctionId;
        }
    }
}
