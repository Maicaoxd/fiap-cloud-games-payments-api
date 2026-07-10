using PaymentsAPI.Options;
using Shouldly;

namespace PaymentsAPI.Tests.Options
{
    public sealed class RabbitMqOptionsTests
    {
        [Fact]
        public void RabbitMqOptions_WhenNotConfigured_UsesLocalDevelopmentDefaults()
        {
            var options = new RabbitMqOptions();

            options.Host.ShouldBe("localhost");
            options.Port.ShouldBe(5672);
            options.VirtualHost.ShouldBe("/");
            options.Username.ShouldBe("guest");
            options.Password.ShouldBe("guest");
        }
    }
}
