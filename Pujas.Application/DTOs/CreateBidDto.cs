
namespace Pujas.Application.DTOs
{
    public class CreateBidDto
    {
        public string UserId { get; init; }
        public string AuctionId { get; init; }
        public decimal Price { get; init; }
        public DateTime Date { get; init; }
    }
}
