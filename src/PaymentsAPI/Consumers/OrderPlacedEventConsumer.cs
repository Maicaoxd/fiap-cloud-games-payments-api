using FiapCloudGames.Contracts.Events;
using MassTransit;
using PaymentsAPI.Application.Payments.ProcessOrderPayment;

namespace PaymentsAPI.Consumers
{
    public sealed class OrderPlacedEventConsumer : IConsumer<OrderPlacedEvent>
    {
        private readonly ILogger<OrderPlacedEventConsumer> _logger;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly IProcessOrderPaymentUseCase _processOrderPaymentUseCase;

        public OrderPlacedEventConsumer(
            ILogger<OrderPlacedEventConsumer> logger,
            IPublishEndpoint publishEndpoint,
            IProcessOrderPaymentUseCase processOrderPaymentUseCase)
        {
            _logger = logger;
            _publishEndpoint = publishEndpoint;
            _processOrderPaymentUseCase = processOrderPaymentUseCase;
        }

        public async Task Consume(ConsumeContext<OrderPlacedEvent> context)
        {
            var orderPlacedEvent = context.Message;

            _logger.LogInformation(
                "OrderPlacedEvent recebido. OrderId: {OrderId}, UserId: {UserId}, Jogos: {GameCount}, Total: {TotalPrice}",
                orderPlacedEvent.OrderId,
                orderPlacedEvent.UserId,
                orderPlacedEvent.Games.Count,
                orderPlacedEvent.TotalPrice);

            var paymentProcessedEvent = _processOrderPaymentUseCase.Execute(orderPlacedEvent);

            await _publishEndpoint.Publish(paymentProcessedEvent, context.CancellationToken);

            _logger.LogInformation(
                "PaymentProcessedEvent publicado. OrderId: {OrderId}, UserId: {UserId}, Status: {Status}",
                paymentProcessedEvent.OrderId,
                paymentProcessedEvent.UserId,
                paymentProcessedEvent.Status);
        }
    }
}
