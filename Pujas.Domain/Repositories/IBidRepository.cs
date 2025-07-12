using Pujas.Domain.Aggregates;

namespace Pujas.Domain.Repositories
{
    public interface IBidRepository
    {
        Task CreateBid(Bid Bid);
        Task<Bid?> GetBidById(string id);

    }
}
