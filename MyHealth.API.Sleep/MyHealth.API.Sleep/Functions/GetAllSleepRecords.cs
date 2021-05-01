using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MyHealth.API.Sleep.Services;
using MyHealth.Common;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using mdl = MyHealth.Common.Models;

namespace MyHealth.API.Sleep.Functions
{
    public class GetAllSleepRecords
    {
        private readonly ISleepDbService _sleepDbService;
        private readonly IServiceBusHelpers _serviceBusHelpers;
        private readonly IConfiguration _configuration;

        public GetAllSleepRecords(
            ISleepDbService sleepDbService,
            IServiceBusHelpers serviceBusHelpers,
            IConfiguration configuration)
        {
            _sleepDbService = sleepDbService ?? throw new ArgumentNullException(nameof(sleepDbService));
            _serviceBusHelpers = serviceBusHelpers ?? throw new ArgumentNullException(nameof(serviceBusHelpers));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
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
                await _serviceBusHelpers.SendMessageToQueue(_configuration["ExceptionQueue"], ex);
                result = new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }

            return result;
        }
    }
}
