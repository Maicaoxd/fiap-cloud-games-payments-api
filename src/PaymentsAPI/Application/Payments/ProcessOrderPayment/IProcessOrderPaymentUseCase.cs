using FiapCloudGames.Contracts.Events;

namespace PaymentsAPI.Application.Payments.ProcessOrderPayment
{
    public interface IProcessOrderPaymentUseCase
    {
        PaymentProcessedEvent Execute(OrderPlacedEvent orderPlacedEvent);
    }
}
