using PaymentsAPI.Options;
using Shouldly;

namespace PaymentsAPI.Tests.Options
{
    public sealed class PaymentSimulationOptionsTests
    {
        [Fact]
        public void DefaultStatus_WhenNotConfigured_IsApproved()
        {
            var options = new PaymentSimulationOptions();

            options.DefaultStatus.ShouldBe("Approved");
        }

        [Fact]
        public void AllowedStatuses_ContainsApprovedAndRejected()
        {
            PaymentSimulationOptions.AllowedStatuses.ShouldContain("Approved");
            PaymentSimulationOptions.AllowedStatuses.ShouldContain("Rejected");
        }
    }
}
