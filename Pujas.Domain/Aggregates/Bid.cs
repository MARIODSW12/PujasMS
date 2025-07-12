using Pujas.Domain.Exceptions;
using Pujas.Domain.ValueObjects;

namespace Pujas.Domain.Aggregates
{
    public class Bid
    {
        public VOId Id { get; private set; }
        public VOUserId UserId { get; private set; }
        public VOAuctionId AuctionId { get; private set; }
        public VOPrice Price { get; private set; }
        public VODate Date { get; private set; }


        public Bid(VOId id, VOUserId userId, VOPrice price, VODate date, VOAuctionId auctionId)
        {
            Id = id;
            UserId = userId;
            Price = price;
            Date = date;
            AuctionId = auctionId;
        }

    }
}