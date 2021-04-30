using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using MyHealth.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyHealth.API.Sleep.Services
{
    public class SleepDbService : ISleepDbService
    {
        private readonly IConfiguration _configuration;
        private readonly CosmosClient _cosmosClient;
        private readonly Container _container;

        public SleepDbService(
            IConfiguration configuration,
            CosmosClient cosmosClient)
        {
            _configuration = configuration;
            _cosmosClient = cosmosClient;
            _container = _cosmosClient.GetContainer(_configuration["DatabaseName"], _configuration["ContainerName"]);
        }

        public async Task<SleepEnvelope> GetSleepRecordByDate(string sleepDate)
        {
            try
            {
                QueryDefinition query = new QueryDefinition("SELECT * FROM Records c WHERE c.DocumentType = 'Sleep' AND c.Sleep.SleepDate = @sleepDate")
                    .WithParameter("@sleepDate", sleepDate);

                List<SleepEnvelope> sleepEnvelopes = new List<SleepEnvelope>();

                FeedIterator<SleepEnvelope> feedIterator = _container.GetItemQueryIterator<SleepEnvelope>(query);

                while (feedIterator.HasMoreResults)
                {
                    FeedResponse<SleepEnvelope> queryResponse = await feedIterator.ReadNextAsync();
                    sleepEnvelopes.AddRange(queryResponse.Resource);
                }

                return sleepEnvelopes.FirstOrDefault();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<List<SleepEnvelope>> GetSleepRecords()
        {
            try
            {
                QueryDefinition query = new QueryDefinition("SELECT * FROM Records c WHERE c.DocumentType = 'Sleep'");

                List<SleepEnvelope> sleepEnvelopes = new List<SleepEnvelope>();

                FeedIterator<SleepEnvelope> feedIterator = _container.GetItemQueryIterator<SleepEnvelope>(query);

                while (feedIterator.HasMoreResults)
                {
                    FeedResponse<SleepEnvelope> queryResponse = await feedIterator.ReadNextAsync();
                    sleepEnvelopes.AddRange(queryResponse.Resource);
                }

                return sleepEnvelopes;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
