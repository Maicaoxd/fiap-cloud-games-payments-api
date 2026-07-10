namespace PaymentsAPI.Options
{
    public sealed class PaymentSimulationOptions
    {
        public const string SectionName = "PaymentSimulation";

        public static readonly string[] AllowedStatuses = ["Approved", "Rejected"];

        public string DefaultStatus { get; init; } = "Approved";
    }
}
