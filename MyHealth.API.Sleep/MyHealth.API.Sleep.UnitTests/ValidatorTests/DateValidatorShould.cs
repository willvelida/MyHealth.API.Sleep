using MyHealth.API.Sleep.Validators;
using Xunit;

namespace MyHealth.API.Sleep.UnitTests.ValidatorTests
{
    public class DateValidatorShould
    {
        private DateValidator _sut;

        public DateValidatorShould()
        {
            _sut = new DateValidator();
        }

        [Fact]
        public void ReturnFalseIfSleepDateIsNotInValidFormat()
        {
            // Arrange
            string testSleepDate = "2021-12-100";

            // Act
            var response = _sut.IsSleepDateValid(testSleepDate);

            // Assert
            Assert.False(response);
        }

        [Fact]
        public void ReturnTrueIfSleepDateIsInValidFormat()
        {
            // Arrange
            string testSleepDate = "2020-12-31";

            // Act
            var response = _sut.IsSleepDateValid(testSleepDate);

            // Assert
            Assert.True(response);
        }
    }
}
