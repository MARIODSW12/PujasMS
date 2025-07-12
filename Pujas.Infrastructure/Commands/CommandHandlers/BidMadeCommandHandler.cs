using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using Pujas.Application.Commands;
using Pujas.Application.DTOs;
using Pujas.Infrastructure.Interfaces;

namespace Pujas.Infrastructure.Commands.CommandHandlers
{
    public class BidMadeCommandHandler(IServiceProvider serviceProvider, IReadAutomaticBidRepository bidAutomaticReadRepository, IMediator mediator) : IRequestHandler<BidMadeCommand, List<GetBidDto>>
    {
        private readonly IServiceProvider _serviceProvider = serviceProvider;
        private readonly IReadAutomaticBidRepository _bidAutomaticReadRepository = bidAutomaticReadRepository;
        private readonly IMediator _mediator = mediator;

        public async Task<List<GetBidDto>> Handle(BidMadeCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var autoBids = await _bidAutomaticReadRepository.GetAuctionAutomaticBids(request.AuctionId, request.MinimumIncrease + request.Price);
                var actualBid = request.Price;
                var createdBids = new List<GetBidDto>();

                foreach (var bid in autoBids)
                {
                    if (bid["limit"].AsDecimal < actualBid + bid["increase"].AsDecimal)
                        continue;
                    if (bid["minimum"].AsDecimal > actualBid)
                        continue;
                    var bidDto = new CreateBidDto
                    {
                        AuctionId = request.AuctionId,
                        UserId = bid["userId"].AsString,
                        Price = actualBid + bid["increase"].AsDecimal,
                        Date = DateTime.UtcNow
                    };

                    var bidId = await _mediator.Send(new CreateBidCommand(bidDto));
                    createdBids.Add(new GetBidDto
                    {
                        Id = bidId,
                        UserId = bidDto.UserId,
                        AuctionId = bidDto.AuctionId,
                        Price = bidDto.Price,
                        Date = bidDto.Date
                    });
                    actualBid = actualBid + bid["increase"].AsDecimal;
                }

                return createdBids;
            }
            catch (Exception ex)
            {
                throw; // Consider logging the exception
            }
        }
    }
}
