using FiapCloudGames.Contracts.Events;
using MassTransit;
using Microsoft.Extensions.Logging;
using NSubstitute;
using PaymentsAPI.Application.Payments.ProcessOrderPayment;
using PaymentsAPI.Consumers;
using Shouldly;

namespace PaymentsAPI.Tests.Consumers
{
    public sealed class OrderPlacedEventConsumerTests
    {
        [Fact]
        public async Task Consume_WhenOrderIsPlaced_PublishesPaymentProcessedEvent()
        {
            var logger = new TestLogger<OrderPlacedEventConsumer>();
            var publishEndpoint = Substitute.For<IPublishEndpoint>();
            var useCase = Substitute.For<IProcessOrderPaymentUseCase>();
            var consumer = new OrderPlacedEventConsumer(logger, publishEndpoint, useCase);
            var orderPlacedEvent = CreateOrderPlacedEvent();
            var paymentProcessedEvent = CreatePaymentProcessedEvent(orderPlacedEvent, "Approved");
            var context = Substitute.For<ConsumeContext<OrderPlacedEvent>>();

            context.Message.Returns(orderPlacedEvent);
            context.CancellationToken.Returns(CancellationToken.None);
            useCase.Execute(orderPlacedEvent).Returns(paymentProcessedEvent);

            await consumer.Consume(context);

            useCase.Received(1).Execute(orderPlacedEvent);
            await publishEndpoint.Received(1).Publish(paymentProcessedEvent, CancellationToken.None);
            logger.Messages.ShouldContain(log =>
                log.Contains("OrderPlacedEvent recebido", StringComparison.OrdinalIgnoreCase) &&
                log.Contains(orderPlacedEvent.OrderId.ToString(), StringComparison.OrdinalIgnoreCase));
            logger.Messages.ShouldContain(log =>
                log.Contains("PaymentProcessedEvent publicado", StringComparison.OrdinalIgnoreCase) &&
                log.Contains("Approved", StringComparison.OrdinalIgnoreCase));
        }

        private static OrderPlacedEvent CreateOrderPlacedEvent()
        {
            return new OrderPlacedEvent(
                Guid.NewGuid(),
                Guid.NewGuid(),
                new[]
                {
                    new OrderPlacedGameEventItem(Guid.NewGuid(), 49.90m),
                    new OrderPlacedGameEventItem(Guid.NewGuid(), 24.90m)
                },
                74.80m,
                DateTime.UtcNow);
        }

        private static PaymentProcessedEvent CreatePaymentProcessedEvent(
            OrderPlacedEvent orderPlacedEvent,
            string status)
        {
            return new PaymentProcessedEvent(
                orderPlacedEvent.OrderId,
                orderPlacedEvent.UserId,
                orderPlacedEvent.Games.Select(game => new PaymentProcessedGameEventItem(game.GameId, game.Price)).ToArray(),
                orderPlacedEvent.TotalPrice,
                status,
                DateTime.UtcNow);
        }

        private sealed class TestLogger<T> : ILogger<T>
        {
            public List<string> Messages { get; } = new();

            public IDisposable? BeginScope<TState>(TState state)
                where TState : notnull
            {
                return NullScope.Instance;
            }

            public bool IsEnabled(LogLevel logLevel)
            {
                return true;
            }

            public void Log<TState>(
                LogLevel logLevel,
                EventId eventId,
                TState state,
                Exception? exception,
                Func<TState, Exception?, string> formatter)
            {
                Messages.Add(formatter(state, exception));
            }

            private sealed class NullScope : IDisposable
            {
                public static readonly NullScope Instance = new();

                public void Dispose()
                {
                }
            }
        }
    }
}
