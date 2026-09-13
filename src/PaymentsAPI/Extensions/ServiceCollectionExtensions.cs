using MassTransit;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using PaymentsAPI.Application.Payments.ProcessOrderPayment;
using PaymentsAPI.Consumers;
using PaymentsAPI.Health;
using PaymentsAPI.Options;

namespace PaymentsAPI.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApiPresentation(this IServiceCollection services)
        {
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "FIAP Cloud Games Payments API",
                    Version = "v1",
                    Description = "Microservico responsavel por simular pagamentos via eventos."
                });
            });

            return services;
        }

        public static IServiceCollection AddMessaging(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services
                .AddOptions<RabbitMqOptions>()
                .Bind(configuration.GetSection(RabbitMqOptions.SectionName))
                .Validate(options => !string.IsNullOrWhiteSpace(options.Host), "RabbitMq:Host é obrigatório.")
                .Validate(options => options.Port is > 0 and <= 65535, "RabbitMq:Port deve estar entre 1 e 65535.")
                .Validate(options => !string.IsNullOrWhiteSpace(options.VirtualHost), "RabbitMq:VirtualHost é obrigatório.")
                .Validate(options => !string.IsNullOrWhiteSpace(options.Username), "RabbitMq:Username é obrigatório.")
                .Validate(options => !string.IsNullOrWhiteSpace(options.Password), "RabbitMq:Password é obrigatório.")
                .ValidateOnStart();

            services
                .AddOptions<PaymentSimulationOptions>()
                .Bind(configuration.GetSection(PaymentSimulationOptions.SectionName))
                .Validate(
                    options => PaymentSimulationOptions.AllowedStatuses.Contains(options.DefaultStatus, StringComparer.OrdinalIgnoreCase),
                    "PaymentSimulation:DefaultStatus deve ser Approved ou Rejected.")
                .ValidateOnStart();

            services.AddScoped<IProcessOrderPaymentUseCase, ProcessOrderPaymentUseCase>();
            services.AddSingleton<IRabbitMqConnectionChecker, RabbitMqConnectionChecker>();

            services.AddMassTransit(x =>
            {
                x.SetKebabCaseEndpointNameFormatter();
                x.AddConsumer<OrderPlacedEventConsumer>()
                    .Endpoint(endpoint => endpoint.Name = "payments-order-placed-event");

                x.UsingRabbitMq((context, cfg) =>
                {
                    var rabbitMqOptions = context.GetRequiredService<IOptions<RabbitMqOptions>>().Value;
                    var virtualHostPath = rabbitMqOptions.VirtualHost == "/"
                        ? string.Empty
                        : Uri.EscapeDataString(rabbitMqOptions.VirtualHost.TrimStart('/'));

                    var hostAddress = new UriBuilder("rabbitmq", rabbitMqOptions.Host, rabbitMqOptions.Port, virtualHostPath).Uri;

                    cfg.Host(hostAddress, h =>
                    {
                        h.Username(rabbitMqOptions.Username);
                        h.Password(rabbitMqOptions.Password);
                    });

                    cfg.ConfigureEndpoints(context);
                });
            });

            return services;
        }
    }
}
