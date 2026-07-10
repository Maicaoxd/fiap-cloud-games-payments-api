using Microsoft.Extensions.Options;
using PaymentsAPI.Health;
using PaymentsAPI.Options;
using Shouldly;
using System.Net;
using System.Net.Sockets;

namespace PaymentsAPI.Tests.Health
{
    public sealed class RabbitMqConnectionCheckerTests
    {
        [Fact]
        public async Task CanConnectAsync_WhenTcpPortIsOpen_ReturnsTrue()
        {
            using var listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            var endpoint = (IPEndPoint)listener.LocalEndpoint;
            var checker = CreateChecker("127.0.0.1", endpoint.Port);

            var result = await checker.CanConnectAsync();

            result.ShouldBeTrue();
        }

        [Fact]
        public async Task CanConnectAsync_WhenTcpPortIsClosed_ReturnsFalse()
        {
            var closedPort = GetClosedPort();
            var checker = CreateChecker("127.0.0.1", closedPort);

            var result = await checker.CanConnectAsync();

            result.ShouldBeFalse();
        }

        private static RabbitMqConnectionChecker CreateChecker(string host, int port)
        {
            return new RabbitMqConnectionChecker(Microsoft.Extensions.Options.Options.Create(new RabbitMqOptions
            {
                Host = host,
                Port = port
            }));
        }

        private static int GetClosedPort()
        {
            using var listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            var port = ((IPEndPoint)listener.LocalEndpoint).Port;
            listener.Stop();
            return port;
        }
    }
}
