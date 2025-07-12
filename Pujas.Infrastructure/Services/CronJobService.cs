using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hangfire;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Pujas.Infrastructure.Commands;
using Pujas.Infrastructure.Dtos;
using Pujas.Infrastructure.interfaces;
using Pujas.Infrastructure.Interfaces;

namespace Pujas.Infrastructure.Services
{
    public class CronJobService: ICronJobService
    {
        private static List<CronPujasId> cronPujasIds = [];
        private readonly object _lock = new();
        private readonly IMediator _mediator;
        private readonly IBidHub BidHub;
        public CronJobService(IMediator mediator, IBidHub bidHub)
        {
            _mediator = mediator;
            BidHub = bidHub;
        }

        public void AddCronJob(string auctionId, string cronId)
        {

                var existingCronJob = cronPujasIds.FirstOrDefault(c => c.AuctionId == auctionId);
                if (existingCronJob != null)
                {
                    existingCronJob.Id = cronId;
                    return;
                }
                
                cronPujasIds.Add(new CronPujasId { AuctionId = auctionId, Id = cronId });
            
        }

        public void DeleteCronJob(string auctionId)
        {
                var cronJob = cronPujasIds.FirstOrDefault(c => c.AuctionId == auctionId);
                if (cronJob != null)
                {
                    cronPujasIds.Remove(cronJob);
                    BackgroundJob.Delete(cronJob.Id);
                }
            
        }

        public void CreateCronJob(string auctionId, decimal price, decimal minimumIncrease)
        {
                var time = DateTime.UtcNow.AddSeconds((double.Parse(Environment.GetEnvironmentVariable("SECONDS_BETWEEN_BIDS"))));
                TimeSpan delay = time - DateTime.UtcNow;
                var cronId = BackgroundJob.Schedule(() => TriggerAutomaticBids(auctionId, minimumIncrease, price), delay);
                AddCronJob(auctionId, cronId);
            
        }

        async public Task TriggerAutomaticBids(string auctionId, decimal minimumIncrease, decimal price)
        {
            var autoBids = await _mediator.Send(new BidMadeCommand(null, price, auctionId, minimumIncrease));
            DeleteCronJob(auctionId);
            CreateCronJob(auctionId, autoBids.LastOrDefault()?.Price ?? price, minimumIncrease);
            foreach (var bid in autoBids)
            {
                var message = new WebSocketMessage
                {
                    Event = "bidMade",
                    Data = new { Id = bid.Id, UserId = bid.UserId, AuctionId = bid.AuctionId, Price = bid.Price, Date = bid.Date }
                };
                await BidHub.SendMessageToAll(message);
            }
        }
    }
}
