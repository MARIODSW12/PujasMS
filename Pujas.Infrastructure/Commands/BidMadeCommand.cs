using MediatR;
using Pujas.Application.DTOs;

namespace Pujas.Infrastructure.Commands
{
    public class BidMadeCommand : IRequest<List<GetBidDto>>
    {
        public string? UserId { get; }
        public string AuctionId { get; }
        public decimal Price { get; }
        public decimal MinimumIncrease { get; }

        public BidMadeCommand(string? userId, decimal price, string auctionId, decimal minimumIncrease)
        {
            UserId = userId;
            Price = price;
            AuctionId = auctionId;
            MinimumIncrease = minimumIncrease;
        }
    }
}
