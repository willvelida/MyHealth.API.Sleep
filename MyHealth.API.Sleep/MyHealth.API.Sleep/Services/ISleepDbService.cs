using System.Collections.Generic;
using System.Threading.Tasks;
using mdl = MyHealth.Common.Models;

namespace MyHealth.API.Sleep.Services
{
    public interface ISleepDbService
    {
        /// <summary>
        /// Retrieves all sleep records from the Records container
        /// </summary>
        /// <returns></returns>
        Task<List<mdl.SleepEnvelope>> GetSleepRecords();

        /// <summary>
        /// Gets a sleep record by a provided date
        /// </summary>
        /// <param name="activityDate"></param>
        /// <returns></returns>
        Task<mdl.SleepEnvelope> GetSleepRecordByDate(string sleepDate);
    }
}
