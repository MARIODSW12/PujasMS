using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pujas.Infrastructure.Interfaces
{
    public interface ICronJobService
    {
        void AddCronJob(string auctionId, string cronId);
        void DeleteCronJob(string auctionId);
        void CreateCronJob(string auctionId, decimal price, decimal minimumIncrease);
        Task TriggerAutomaticBids(string auctionId, decimal minimumIncrease, decimal price);
    }
}
