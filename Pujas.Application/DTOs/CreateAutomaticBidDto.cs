
namespace Pujas.Application.DTOs
{
    public class CreateAutomaticBidDto
    {
        public string UserId { get; init; }
        public string AuctionId { get; init; }
        public decimal Limit { get; init; }
        public decimal Increment { get; init; }
        public decimal Minimum { get; init; }
    }
}
