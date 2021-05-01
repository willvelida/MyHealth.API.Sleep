using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MyHealth.API.Sleep.Services;
using MyHealth.API.Sleep.Validators;
using MyHealth.Common;
using System;
using System.Threading.Tasks;

namespace MyHealth.API.Sleep.Functions
{
    public class GetSleepByDate
    {
        private readonly ISleepDbService _sleepDbService;
        private readonly IDateValidator _dateValidator;
        private readonly IServiceBusHelpers _serviceBusHelpers;
        private readonly IConfiguration _configuration;

        public GetSleepByDate(
            ISleepDbService sleepDbService,
            IDateValidator dateValidator,
            IServiceBusHelpers serviceBusHelpers,
            IConfiguration configuration)
        {
            _sleepDbService = sleepDbService ?? throw new ArgumentNullException(nameof(sleepDbService));
            _serviceBusHelpers = serviceBusHelpers ?? throw new ArgumentNullException(nameof(serviceBusHelpers));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _dateValidator = dateValidator ?? throw new ArgumentNullException(nameof(dateValidator));
        }

        [FunctionName(nameof(GetSleepByDate))]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "Sleep")] HttpRequest req,
            ILogger log)
        {
            IActionResult result;

            try
            {
                string sleepDate = req.Query["date"];

                bool isDateValid = _dateValidator.IsSleepDateValid(sleepDate);
                if (isDateValid == false)
                {
                    result = new BadRequestResult();
                    return result;
                }

                var sleepResponse = await _sleepDbService.GetSleepRecordByDate(sleepDate);
                if (sleepResponse == null)
                {
                    result = new NotFoundResult();
                    return result;
                }

                var sleep = sleepResponse.Sleep;

                result = new OkObjectResult(sleep);
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
