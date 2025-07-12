using MediatR;

using Pujas.Application.DTOs;

namespace Pujas.Infrastructure.Queries
{
    public class GetUserParticipateAuctionsQuery : IRequest<List<GetBidDto>>
    {
        public string UserId { get; set; }
        public List<string>? AuctionIds { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public GetUserParticipateAuctionsQuery(string userId, List<string>? auctionIds, DateTime? from, DateTime? to)
        {
            UserId = userId;
            AuctionIds = auctionIds;
            From = from;
            To = to;
        }
    }
}
