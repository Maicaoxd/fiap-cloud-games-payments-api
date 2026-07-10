using FiapCloudGames.Contracts.Events;
using Microsoft.Extensions.Options;
using PaymentsAPI.Application.Payments.ProcessOrderPayment;
using PaymentsAPI.Options;
using Shouldly;

namespace PaymentsAPI.Tests.Application.Payments.ProcessOrderPayment
{
    public sealed class ProcessOrderPaymentUseCaseTests
    {
        [Fact]
        public void Execute_WhenDefaultStatusIsApproved_ReturnsApprovedPaymentProcessedEvent()
        {
            var options = Microsoft.Extensions.Options.Options.Create(new PaymentSimulationOptions { DefaultStatus = "Approved" });
            var useCase = new ProcessOrderPaymentUseCase(options);
            var orderPlacedEvent = CreateOrderPlacedEvent();
            var before = DateTime.UtcNow;

            var result = useCase.Execute(orderPlacedEvent);

            result.OrderId.ShouldBe(orderPlacedEvent.OrderId);
            result.UserId.ShouldBe(orderPlacedEvent.UserId);
            result.TotalPrice.ShouldBe(orderPlacedEvent.TotalPrice);
            result.Status.ShouldBe("Approved");
            result.ProcessedAt.ShouldBeGreaterThanOrEqualTo(before);
            result.Games.Count.ShouldBe(2);
            result.Games.Select(game => game.GameId).ShouldBe(orderPlacedEvent.Games.Select(game => game.GameId));
            result.Games.Select(game => game.Price).ShouldBe(orderPlacedEvent.Games.Select(game => game.Price));
        }

        [Fact]
        public void Execute_WhenDefaultStatusIsRejected_ReturnsRejectedPaymentProcessedEvent()
        {
            var options = Microsoft.Extensions.Options.Options.Create(new PaymentSimulationOptions { DefaultStatus = "Rejected" });
            var useCase = new ProcessOrderPaymentUseCase(options);
            var orderPlacedEvent = CreateOrderPlacedEvent();

            var result = useCase.Execute(orderPlacedEvent);

            result.Status.ShouldBe("Rejected");
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
    }
}
