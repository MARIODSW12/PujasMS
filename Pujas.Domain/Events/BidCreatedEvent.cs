using MediatR;

namespace Pujas.Domain.Events
{
    public class BidCreatedEvent : INotification
    {
        public string BidId { get; }
        public string UserId { get; }
        public string AuctionId { get; }
        public decimal Price { get; }
        public DateTime Date { get; private set; }

        public BidCreatedEvent(string bidId, string userId, decimal price, DateTime date, string auctionId)
        {
            BidId = bidId;
            UserId = userId;
            Price = price;
            Date = date;
            AuctionId = auctionId;
        }
    }
}
