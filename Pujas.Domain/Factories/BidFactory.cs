using Pujas.Domain.Aggregates;
using Pujas.Domain.ValueObjects;

namespace Pujas.Domain.Factories
{
    public static class BidFactory
    {
        public static Bid Create(VOId id, VOUserId userId, VOPrice price, VODate date, VOAuctionId auctionId)
        {
            return new Bid(id, userId, price, date, auctionId);
        }
    }
}
