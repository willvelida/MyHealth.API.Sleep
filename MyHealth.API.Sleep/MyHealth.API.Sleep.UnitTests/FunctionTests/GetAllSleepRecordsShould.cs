using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using MyHealth.API.Sleep.Functions;
using MyHealth.API.Sleep.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using mdl = MyHealth.Common.Models;

namespace MyHealth.API.Sleep.UnitTests.FunctionTests
{
    public class GetAllSleepRecordsShould
    {
        private Mock<ISleepDbService> _mockSleepDbService;
        private Mock<HttpRequest> _mockHttpRequest;
        private Mock<ILogger> _mockLogger;

        private GetAllSleepRecords _func;

        public GetAllSleepRecordsShould()
        {
            _mockSleepDbService = new Mock<ISleepDbService>();
            _mockHttpRequest = new Mock<HttpRequest>();
            _mockLogger = new Mock<ILogger>();

            _func = new GetAllSleepRecords(_mockSleepDbService.Object);
        }

        [Fact]
        public async Task ReturnOkObjectResultWhenSleepRecordsAreFound()
        {
            // Arrange
            var sleeps = new List<mdl.SleepEnvelope>();
            var sleepEnvelope = new mdl.SleepEnvelope
            {
                Id = Guid.NewGuid().ToString(),
                Sleep = new mdl.Sleep
                {
                    SleepDate = "31/12/2019"
                },
                DocumentType = "Sleep"
            };
            sleeps.Add(sleepEnvelope);
            byte[] byteArray = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(sleeps));
            MemoryStream memoryStream = new MemoryStream(byteArray);
            _mockHttpRequest.Setup(r => r.Body).Returns(memoryStream);

            _mockSleepDbService.Setup(x => x.GetSleepRecords()).ReturnsAsync(sleeps);

            // Act
            var response = await _func.Run(_mockHttpRequest.Object, _mockLogger.Object);

            // Assert
            Assert.Equal(typeof(OkObjectResult), response.GetType());
        }

        [Fact]
        public async Task ReturnOkObjectResultWhenNoSleepRecordsAreFound()
        {
            // Arrange
            var sleeps = new List<mdl.SleepEnvelope>();
            byte[] byteArray = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(sleeps));
            MemoryStream memoryStream = new MemoryStream(byteArray);
            _mockHttpRequest.Setup(r => r.Body).Returns(memoryStream);

            _mockSleepDbService.Setup(x => x.GetSleepRecords()).ReturnsAsync(sleeps);

            // Act
            var response = await _func.Run(_mockHttpRequest.Object, _mockLogger.Object);

            // Assert
            Assert.Equal(typeof(OkObjectResult), response.GetType());
        }

        [Fact]
        public async Task Throw500InternalServerErrorStatusCodeWhenSleepDbServiceThrowsException()
        {
            // Arrange
            var sleeps = new List<mdl.SleepEnvelope>();
            byte[] byteArray = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(sleeps));
            MemoryStream memoryStream = new MemoryStream(byteArray);
            _mockHttpRequest.Setup(r => r.Body).Returns(memoryStream);

            _mockSleepDbService.Setup(x => x.GetSleepRecords()).ThrowsAsync(new Exception());

            // Act
            var response = await _func.Run(_mockHttpRequest.Object, _mockLogger.Object);

            // Assert
            Assert.Equal(typeof(StatusCodeResult), response.GetType());
            var responseAsStatusCodeResult = (StatusCodeResult)response;
            Assert.Equal(500, responseAsStatusCodeResult.StatusCode);
        }
    }
}
