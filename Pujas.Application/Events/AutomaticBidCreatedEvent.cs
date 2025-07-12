using MediatR;

namespace Pujas.Application.Events
{
    public class AutomaticBidCreatedEvent : INotification
    {
        public string UserId { get; }
        public string AuctionId { get; }
        public decimal Limit { get; }
        public decimal Minimum { get; }
        public decimal Increase { get; }
        public DateTime Date { get; private set; }

        public AutomaticBidCreatedEvent(string userId, decimal limit, DateTime date, string auctionId, decimal minimum, decimal increase)
        {
            UserId = userId;
            Limit = limit;
            Date = date;
            AuctionId = auctionId;
            Minimum = minimum;
            Increase = increase;
        }
    }
}
