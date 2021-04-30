using FluentAssertions;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Moq;
using MyHealth.API.Sleep.Services;
using MyHealth.API.Sleep.UnitTests.TestHelpers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using mdl = MyHealth.Common.Models;

namespace MyHealth.API.Sleep.UnitTests.ServiceTests
{
    public class SleepDbServiceShould
    {
        private Mock<CosmosClient> _mockCosmosClient;
        private Mock<Container> _mockContainer;
        private Mock<IConfiguration> _mockConfiguration;

        private SleepDbService _sut;

        public SleepDbServiceShould()
        {
            _mockCosmosClient = new Mock<CosmosClient>();
            _mockContainer = new Mock<Container>();
            _mockCosmosClient.Setup(c => c.GetContainer(It.IsAny<string>(), It.IsAny<string>())).Returns(_mockContainer.Object);
            _mockConfiguration = new Mock<IConfiguration>();
            _mockConfiguration.Setup(x => x["DatabaseName"]).Returns("db");
            _mockConfiguration.Setup(x => x["ContainerName"]).Returns("col");

            _sut = new SleepDbService(
                _mockConfiguration.Object,
                _mockCosmosClient.Object);
        }

        [Fact]
        public async Task GetAllSleepRecordsSuccessfully()
        {
            // Arrange
            List<mdl.SleepEnvelope> sleepEnvelopes = new List<mdl.SleepEnvelope>();
            mdl.SleepEnvelope sleepEnvelope = new mdl.SleepEnvelope
            {
                Id = Guid.NewGuid().ToString(),
                DocumentType = "Sleep",
                Sleep = new mdl.Sleep
                {
                    SleepDate = "31/12/2019"
                }
            };
            sleepEnvelopes.Add(sleepEnvelope);

            _mockContainer.SetupItemQueryIteratorMock(sleepEnvelopes);
            _mockContainer.SetupItemQueryIteratorMock(new List<int>() { sleepEnvelopes.Count });

            // Act
            var response = await _sut.GetSleepRecords();

            // Assert
            Assert.Equal(sleepEnvelopes.Count, response.Count);
        }

        [Fact]
        public async Task GetAllSleepRecordsSuccessfully_NoResultsReturned()
        {
            // Arrange
            List<mdl.SleepEnvelope> sleepEnvelopes = new List<mdl.SleepEnvelope>();

            _mockContainer.SetupItemQueryIteratorMock(sleepEnvelopes);
            _mockContainer.SetupItemQueryIteratorMock(new List<int>() { sleepEnvelopes.Count });

            // Act
            var response = await _sut.GetSleepRecords();

            // Assert
            Assert.Equal(sleepEnvelopes.Count, response.Count);

        }

        [Fact]
        public async Task CatchExceptionWhenCosmosThrowsExceptionWhenGetSleepRecordsIsCalled()
        {
            // Arrange
            _mockContainer.Setup(x => x.GetItemQueryIterator<mdl.SleepEnvelope>(
                It.IsAny<QueryDefinition>(),
                It.IsAny<string>(),
                It.IsAny<QueryRequestOptions>()))
            .Throws(new Exception());

            // Act
            Func<Task> responseAction = async () => await _sut.GetSleepRecords();

            // Assert
            await responseAction.Should().ThrowAsync<Exception>();
        }

        [Fact]
        public async Task GetSleepByDateSuccessfully()
        {
            // Arrange
            List<mdl.SleepEnvelope> sleepEnvelopes = new List<mdl.SleepEnvelope>();
            mdl.SleepEnvelope sleepEnvelope = new mdl.SleepEnvelope
            {
                Id = Guid.NewGuid().ToString(),
                DocumentType = "Sleep",
                Sleep = new mdl.Sleep
                {
                    SleepDate = "31/12/2019"
                }
            };
            sleepEnvelopes.Add(sleepEnvelope);

            _mockContainer.SetupItemQueryIteratorMock(sleepEnvelopes);
            _mockContainer.SetupItemQueryIteratorMock(new List<int>() { sleepEnvelopes.Count });

            var sleepDate = sleepEnvelope.Sleep.SleepDate;

            // Act
            var response = await _sut.GetSleepRecordByDate(sleepDate);

            // Assert
            Assert.Equal(sleepDate, response.Sleep.SleepDate);
        }

        [Fact]
        public async Task GetSleepByDate_NoResultsReturned()
        {
            // Arrange
            var emptySleepList = new List<mdl.SleepEnvelope>();

            var getSleepByDate = _mockContainer.SetupItemQueryIteratorMock(emptySleepList);
            getSleepByDate.feedIterator.Setup(x => x.HasMoreResults).Returns(false);
            _mockContainer.SetupItemQueryIteratorMock(new List<int>() { 0 });

            // Act
            var response = await _sut.GetSleepRecordByDate("31/12/2019");

            // Assert
            Assert.Null(response);
        }

        [Fact]
        public async Task CatchExceptionWhenCosmosThrowsExceptionWhenGetSleepByDateIsCalled()
        {
            // Arrange
            _mockContainer.Setup(x => x.GetItemQueryIterator<mdl.SleepEnvelope>(
                It.IsAny<QueryDefinition>(),
                It.IsAny<string>(),
                It.IsAny<QueryRequestOptions>()))
            .Throws(new Exception());

            // Act
            Func<Task> responseAction = async () => await _sut.GetSleepRecordByDate("31/12/2019");

            // Assert
            await responseAction.Should().ThrowAsync<Exception>();
        }
    }
}
