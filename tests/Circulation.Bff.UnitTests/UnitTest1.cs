using System;
using NSubstitute;
using Shouldly;
using Xunit;

namespace Circulation.Bff.UnitTests
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            var mock = Substitute.For<IServiceProvider>();

            mock.GetService(typeof(string)).Returns("result");

            var result = mock.GetService(typeof(string));

            result.ShouldBe("result");
        }
    }
}
