using System;
using System.Data;
using Ro.Npgsql.Data;
using Xunit;

namespace Ro.Npgsql.Data.Tests
{
    public class CmdBuilderHelpersTests
    {
        [Theory]
        [InlineData(typeof(int), DbType.Int32)]
        [InlineData(typeof(string), DbType.String)]
        [InlineData(typeof(DateTime), DbType.DateTime)]
        [InlineData(typeof(decimal), DbType.Decimal)]
        [InlineData(typeof(bool), DbType.Boolean)]
        [InlineData(typeof(Guid), DbType.Guid)]
        [InlineData(typeof(byte[]), DbType.Binary)]
        [InlineData(typeof(long), DbType.Int64)]
        [InlineData(typeof(double), DbType.Double)]
        [InlineData(typeof(float), DbType.Single)]
        [InlineData(typeof(short), DbType.Int16)]
        [InlineData(typeof(byte), DbType.Byte)]
        [InlineData(typeof(DateTimeOffset), DbType.DateTimeOffset)]
        [InlineData(typeof(TimeSpan), DbType.Time)]
        [InlineData(typeof(object), DbType.Object)]
        public void ToDbType_ShouldReturnCorrectDbType(Type inputType, DbType expectedDbType)
        {
            // Act
            var actualDbType = CmdBuilderHelpers.ToDbType(inputType);

            // Assert
            Assert.Equal(expectedDbType, actualDbType);
        }

        [Fact]
        public void ToParam_WithValue_ShouldCreateCorrectParameter()
        {
            // Arrange
            var value = 123;
            var name = "@id";

            // Act
            var param = value.ToParam(name);

            // Assert
            Assert.Equal(name, param.ParameterName);
            Assert.Equal(value, param.Value);
            Assert.Equal(DbType.Int32, param.DbType);
        }

        [Fact]
        public void ToParam_WithNullValue_ShouldCreateDbNullParameter()
        {
            // Arrange
            string? value = null;
            var name = "@name";

            // Act
            var param = value.ToParam(name);

            // Assert
            Assert.Equal(name, param.ParameterName);
            Assert.Equal(DBNull.Value, param.Value);
            Assert.Equal(DbType.String, param.DbType);
        }
        
        [Fact]
        public void ToParam_WithNullableInt_ShouldCreateCorrectParameter()
        {
            // Arrange
            int? value = 42;
            var name = "@age";

            // Act
            var param = value.ToParam(name);

            // Assert
            Assert.Equal(name, param.ParameterName);
            Assert.Equal(value, param.Value);
            Assert.Equal(DbType.Int32, param.DbType);
        }

        [Fact]
        public void ToParam_WithNullNullableInt_ShouldCreateDbNullParameter()
        {
            // Arrange
            int? value = null;
            var name = "@age";

            // Act
            var param = value.ToParam(name);

            // Assert
            Assert.Equal(name, param.ParameterName);
            Assert.Equal(DBNull.Value, param.Value);
            Assert.Equal(DbType.Int32, param.DbType);
        }

        [Fact]
        public void ToCmd_ShouldCreateCommandWithSqlAndParameters()
        {
            // Arrange
            var sql = "SELECT * FROM users WHERE id = @id;";
            var param = 1.ToParam("@id");

            // Act
            var cmd = sql.ToCmd(param);

            // Assert
            Assert.Equal(sql, cmd.CommandText);
            Assert.Single(cmd.Parameters);
            Assert.Equal(param, cmd.Parameters[0]);
        }
    }
}