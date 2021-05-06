using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using MyHealth.API.Sleep.Functions;
using MyHealth.API.Sleep.Services;
using MyHealth.API.Sleep.Validators;
using MyHealth.Common;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using mdl = MyHealth.Common.Models;

namespace MyHealth.API.Sleep.UnitTests.FunctionTests
{
    public class GetSleepByDateShould
    {
        private Mock<ISleepDbService> _mockSleepDbService;
        private Mock<IDateValidator> _mockDateValidator;
        private Mock<IServiceBusHelpers> _mockServiceBusHelpers;
        private Mock<IConfiguration> _mockConfiguration;
        private Mock<HttpRequest> _mockHttpRequest;
        private Mock<ILogger> _mockLogger;

        private GetSleepByDate _func;

        public GetSleepByDateShould()
        {
            _mockSleepDbService = new Mock<ISleepDbService>();
            _mockDateValidator = new Mock<IDateValidator>();
            _mockServiceBusHelpers = new Mock<IServiceBusHelpers>();
            _mockConfiguration = new Mock<IConfiguration>();
            _mockHttpRequest = new Mock<HttpRequest>();
            _mockLogger = new Mock<ILogger>();

            _func = new GetSleepByDate(
                _mockSleepDbService.Object,
                _mockDateValidator.Object,
                _mockServiceBusHelpers.Object,
                _mockConfiguration.Object);
        }

        [Theory]
        [InlineData("2020-12-100")]
        [InlineData("2020-100-12")]
        [InlineData("20201-12-11")]
        public async Task ThrowBadRequestResultWhenSleepDateRequestIsInvalid(string invalidDateInput)
        {
            // Arrange
            var sleepEnvelope = new mdl.SleepEnvelope();
            byte[] byteArray = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(sleepEnvelope));
            MemoryStream memoryStream = new MemoryStream(byteArray);
            _mockHttpRequest.Setup(r => r.Query["date"]).Returns(invalidDateInput);
            _mockHttpRequest.Setup(r => r.Body).Returns(memoryStream);

            _mockDateValidator.Setup(x => x.IsSleepDateValid(invalidDateInput)).Returns(false);

            // Act
            var response = await _func.Run(_mockHttpRequest.Object, _mockLogger.Object);

            // Assert
            Assert.Equal(typeof(BadRequestResult), response.GetType());
            var responseAsStatusCodeResult = (StatusCodeResult)response;
            Assert.Equal(400, responseAsStatusCodeResult.StatusCode);
            _mockServiceBusHelpers.Verify(x => x.SendMessageToQueue(It.IsAny<string>(), It.IsAny<Exception>()), Times.Never);
        }

        [Fact]
        public async Task ThrowNotFoundResultWhenSleepEnvelopeResponseIsNull()
        {
            // Arrange
            var sleepEnvelope = new mdl.SleepEnvelope();
            byte[] byteArray = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(sleepEnvelope));
            MemoryStream memoryStream = new MemoryStream(byteArray);
            _mockHttpRequest.Setup(r => r.Query["date"]).Returns("2019-12-31");
            _mockHttpRequest.Setup(r => r.Body).Returns(memoryStream);

            _mockDateValidator.Setup(x => x.IsSleepDateValid(It.IsAny<string>())).Returns(true);
            _mockSleepDbService.Setup(x => x.GetSleepRecordByDate(It.IsAny<string>())).Returns(Task.FromResult<mdl.SleepEnvelope>(null));

            // Act
            var response = await _func.Run(_mockHttpRequest.Object, _mockLogger.Object);

            // Assert
            Assert.Equal(typeof(NotFoundResult), response.GetType());
            var responseAsStatusCodeResult = (StatusCodeResult)response;
            Assert.Equal(404, responseAsStatusCodeResult.StatusCode);
            _mockServiceBusHelpers.Verify(x => x.SendMessageToQueue(It.IsAny<string>(), It.IsAny<Exception>()), Times.Never);
        }

        [Fact]
        public async Task ReturnOkObjectResultWhenSleepRecordIsFound()
        {
            // Arrange
            var sleepEnvelope = new mdl.SleepEnvelope
            {
                Id = Guid.NewGuid().ToString(),
                Sleep = new mdl.Sleep
                {
                    SleepDate = "2019-12-31"
                },
                DocumentType = "Test"
            };
            var sleepDate = sleepEnvelope.Sleep.SleepDate;
            byte[] byteArray = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(sleepEnvelope));
            MemoryStream memoryStream = new MemoryStream(byteArray);
            _mockHttpRequest.Setup(r => r.Query["date"]).Returns(sleepDate);
            _mockHttpRequest.Setup(r => r.Body).Returns(memoryStream);

            _mockDateValidator.Setup(x => x.IsSleepDateValid(sleepDate)).Returns(true);
            _mockSleepDbService.Setup(x => x.GetSleepRecordByDate(sleepDate)).ReturnsAsync(sleepEnvelope);

            // Act
            var response = await _func.Run(_mockHttpRequest.Object, _mockLogger.Object);

            // Assert
            Assert.Equal(typeof(OkObjectResult), response.GetType());
            _mockServiceBusHelpers.Verify(x => x.SendMessageToQueue(It.IsAny<string>(), It.IsAny<Exception>()), Times.Never);
        }

        [Fact]
        public async Task Throw500InternalServerErrorStatusCodeWhenSleepDbServiceThrowsException()
        {
            // Arrange
            var sleepEnvelope = new mdl.SleepEnvelope();
            byte[] byteArray = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(sleepEnvelope));
            MemoryStream memoryStream = new MemoryStream(byteArray);
            _mockHttpRequest.Setup(r => r.Query["date"]).Returns("2019-12-31");
            _mockHttpRequest.Setup(r => r.Body).Returns(memoryStream);

            _mockDateValidator.Setup(x => x.IsSleepDateValid(It.IsAny<string>())).Returns(true);
            _mockSleepDbService.Setup(x => x.GetSleepRecordByDate(It.IsAny<string>())).ThrowsAsync(new Exception());

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
