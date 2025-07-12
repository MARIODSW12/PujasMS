using MediatR;

using Pujas.Application.DTOs;

namespace Pujas.Application.Commands
{

    public class CreateBidCommand : IRequest<String>
    {
        public CreateBidDto BidDto { get; }

        public CreateBidCommand(CreateBidDto bidDto)
        {
            BidDto = bidDto ?? throw new ArgumentNullException(nameof(bidDto));
        }
    }
}
