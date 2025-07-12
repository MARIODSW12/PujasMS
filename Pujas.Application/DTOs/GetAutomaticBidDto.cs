
namespace Pujas.Application.DTOs
{
    public class GetAutomaticBidDto
    {
        public string Id { get; init; }
        public string UserId { get; init; }
        public string AuctionId { get; init; }
        public decimal Limit { get; init; }
        public DateTime Date { get; init; }
    }
}
