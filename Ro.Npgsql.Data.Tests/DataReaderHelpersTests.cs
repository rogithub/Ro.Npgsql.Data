using System;
using System.Data;
using Moq;
using Xunit;

namespace Ro.Npgsql.Data.Tests
{
    public class DataReaderHelpersTests
    {
        private readonly Mock<IDataReader> _mockDataReader;

        public DataReaderHelpersTests()
        {
            _mockDataReader = new Mock<IDataReader>();
        }

        [Fact]
        public void GetString_ShouldReturnStringValue()
        {
            // Arrange
            var key = "name";
            var expectedValue = "test";
            _mockDataReader.Setup(dr => dr[key]).Returns(expectedValue);

            // Act
            var result = _mockDataReader.Object.GetString(key);

            // Assert
            Assert.Equal(expectedValue, result);
        }

        [Fact]
        public void GetInt_ShouldReturnIntValue()
        {
            // Arrange
            var key = "age";
            var expectedValue = 30;
            _mockDataReader.Setup(dr => dr[key]).Returns(expectedValue);

            // Act
            var result = _mockDataReader.Object.GetInt(key);

            // Assert
            Assert.Equal(expectedValue, result);
        }

        [Fact]
        public void GetDecimal_ShouldReturnDecimalValue()
        {
            // Arrange
            var key = "price";
            var expectedValue = 19.99m;
            _mockDataReader.Setup(dr => dr[key]).Returns(expectedValue);

            // Act
            var result = _mockDataReader.Object.GetDecimal(key);

            // Assert
            Assert.Equal(expectedValue, result);
        }

        [Fact]
        public void GetGuid_ShouldReturnGuidValue()
        {
            // Arrange
            var key = "id";
            var expectedValue = Guid.NewGuid();
            _mockDataReader.Setup(dr => dr[key]).Returns(expectedValue);

            // Act
            var result = _mockDataReader.Object.GetGuid(key);

            // Assert
            Assert.Equal(expectedValue, result);
        }
        
        [Fact]
        public void GetDate_ShouldReturnDateTimeValue()
        {
            // Arrange
            var key = "created_at";
            var expectedValue = DateTime.Now;
            _mockDataReader.Setup(dr => dr[key]).Returns(expectedValue);

            // Act
            var result = _mockDataReader.Object.GetDate(key);

            // Assert
            Assert.Equal(expectedValue, result);
        }

        [Fact]
        public void GetString_WhenDbNull_ShouldReturnEmptyString()
        {
            // Arrange
            var key = "description";
            _mockDataReader.Setup(dr => dr[key]).Returns(DBNull.Value);

            // Act
            var result = _mockDataReader.Object.GetString(key);

            // Assert
            Assert.Equal(string.Empty, result);
        }
    }
}