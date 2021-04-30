using MyHealth.API.Sleep.Validators;
using System;
using System.Collections.Generic;
using System.Text;
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
        public void ReturnFalseIfActivityDateIsNotInValidFormat()
        {
            // Arrange
            string testSleepDate = "100/12/2021";

            // Act
            var response = _sut.IsSleepDateValid(testSleepDate);

            // Assert
            Assert.False(response);
        }

        [Fact]
        public void ReturnTrueIfActivityDateIsInValidFormat()
        {
            // Arrange
            string testSleepDate = "31/12/2020";

            // Act
            var response = _sut.IsSleepDateValid(testSleepDate);

            // Assert
            Assert.True(response);
        }
    }
}
