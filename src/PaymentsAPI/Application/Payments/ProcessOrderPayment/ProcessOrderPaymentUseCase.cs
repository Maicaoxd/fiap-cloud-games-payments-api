using FiapCloudGames.Contracts.Events;
using Microsoft.Extensions.Options;
using PaymentsAPI.Options;

namespace PaymentsAPI.Application.Payments.ProcessOrderPayment
{
    public sealed class ProcessOrderPaymentUseCase : IProcessOrderPaymentUseCase
    {
        private readonly PaymentSimulationOptions _options;

        public ProcessOrderPaymentUseCase(IOptions<PaymentSimulationOptions> options)
        {
            _options = options.Value;
        }

        public PaymentProcessedEvent Execute(OrderPlacedEvent orderPlacedEvent)
        {
            var games = orderPlacedEvent.Games
                .Select(game => new PaymentProcessedGameEventItem(game.GameId, game.Price))
                .ToArray();

            return new PaymentProcessedEvent(
                orderPlacedEvent.OrderId,
                orderPlacedEvent.UserId,
                games,
                orderPlacedEvent.TotalPrice,
                _options.DefaultStatus,
                DateTime.UtcNow);
        }
    }
}
