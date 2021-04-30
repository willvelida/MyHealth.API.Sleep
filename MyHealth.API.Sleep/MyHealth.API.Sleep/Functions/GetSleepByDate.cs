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
using MyHealth.API.Sleep.Validators;

namespace MyHealth.API.Sleep.Functions
{
    public class GetSleepByDate
    {
        private readonly ISleepDbService _sleepDbService;
        private readonly IDateValidator _dateValidator;

        public GetSleepByDate(
            ISleepDbService sleepDbService,
            IDateValidator dateValidator)
        {
            _sleepDbService = sleepDbService;
            _dateValidator = dateValidator;
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
                result = new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }

            return result;
        }
    }
}
