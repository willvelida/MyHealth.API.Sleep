using AutoFixture;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using MyHealth.API.Sleep.Functions;
using MyHealth.API.Sleep.Services;
using MyHealth.Common;
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
        private Mock<IServiceBusHelpers> _mockServiceBusHelpers;
        private Mock<IConfiguration> _mockConfiguration;
        private Mock<HttpRequest> _mockHttpRequest;
        private Mock<ILogger> _mockLogger;

        private GetAllSleepRecords _func;

        public GetAllSleepRecordsShould()
        {
            _mockSleepDbService = new Mock<ISleepDbService>();
            _mockServiceBusHelpers = new Mock<IServiceBusHelpers>();
            _mockConfiguration = new Mock<IConfiguration>();
            _mockHttpRequest = new Mock<HttpRequest>();
            _mockLogger = new Mock<ILogger>();

            _func = new GetAllSleepRecords(
                _mockSleepDbService.Object,
                _mockServiceBusHelpers.Object,
                _mockConfiguration.Object);
        }

        [Fact]
        public async Task ReturnOkObjectResultWhenSleepRecordsAreFound()
        {
            // Arrange
            var sleeps = new List<mdl.SleepEnvelope>();
            var fixture = new Fixture();
            var sleepEnvelope = fixture.Create<mdl.SleepEnvelope>();
            sleeps.Add(sleepEnvelope);
            byte[] byteArray = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(sleeps));
            MemoryStream memoryStream = new MemoryStream(byteArray);
            _mockHttpRequest.Setup(r => r.Body).Returns(memoryStream);

            _mockSleepDbService.Setup(x => x.GetSleepRecords()).ReturnsAsync(sleeps);

            // Act
            var response = await _func.Run(_mockHttpRequest.Object, _mockLogger.Object);

            // Assert
            Assert.Equal(typeof(OkObjectResult), response.GetType());
            _mockServiceBusHelpers.Verify(x => x.SendMessageToQueue(It.IsAny<string>(), It.IsAny<Exception>()), Times.Never);
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
            _mockServiceBusHelpers.Verify(x => x.SendMessageToQueue(It.IsAny<string>(), It.IsAny<Exception>()), Times.Never);
        }

        [Fact]
        public async Task ThrowBadRequestResultWhenSleepEnvelopesAreNull()
        {
            // Arrange
            MemoryStream memoryStream = new MemoryStream();
            _mockHttpRequest.Setup(r => r.Body).Returns(memoryStream);
            _mockSleepDbService.Setup(x => x.GetSleepRecords()).Returns(Task.FromResult<List<mdl.SleepEnvelope>>(null));

            // Act
            var response = await _func.Run(_mockHttpRequest.Object, _mockLogger.Object);

            // Assert
            Assert.Equal(typeof(NotFoundResult), response.GetType());
            var responseAsStatusCodeResult = (StatusCodeResult)response;
            Assert.Equal(404, responseAsStatusCodeResult.StatusCode);
            _mockServiceBusHelpers.Verify(x => x.SendMessageToQueue(It.IsAny<string>(), It.IsAny<Exception>()), Times.Never);
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
            _mockServiceBusHelpers.Verify(x => x.SendMessageToQueue(It.IsAny<string>(), It.IsAny<Exception>()), Times.Once);
        }
    }
}
