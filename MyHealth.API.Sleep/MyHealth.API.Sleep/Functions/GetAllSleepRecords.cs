using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using MyHealth.API.Sleep.Services;
using mdl = MyHealth.Common.Models;
using System.Collections.Generic;

namespace MyHealth.API.Sleep.Functions
{
    public class GetAllSleepRecords
    {
        private readonly ISleepDbService _sleepDbService;

        public GetAllSleepRecords(
            ISleepDbService sleepDbService)
        {
            _sleepDbService = sleepDbService ?? throw new ArgumentNullException(nameof(sleepDbService));
        }

        [FunctionName(nameof(GetAllSleepRecords))]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "Sleeps")] HttpRequest req,
            ILogger log)
        {
            IActionResult result;

            try
            {
                List<mdl.Sleep> sleeps = new List<mdl.Sleep>();

                var sleepResponses = await _sleepDbService.GetSleepRecords();

                foreach (var item in sleepResponses)
                {
                    sleeps.Add(item.Sleep);
                }

                result = new OkObjectResult(sleeps);
            }
            catch (Exception ex)
            {
                log.LogError($"Internal Server Error. Exception thrown: {ex.Message}");
                result = new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }

            return result;
        }
    }
}
