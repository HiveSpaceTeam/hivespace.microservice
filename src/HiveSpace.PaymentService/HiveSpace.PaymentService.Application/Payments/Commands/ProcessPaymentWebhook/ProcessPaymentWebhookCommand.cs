using HiveSpace.Application.Shared.Commands;
using HiveSpace.PaymentService.Domain.Aggregates.Payments.Enumerations;

namespace HiveSpace.PaymentService.Application.Payments.Commands.ProcessPaymentWebhook;

public record ProcessPaymentWebhookCommand(
    Guid PaymentId,
    Dictionary<string, string> Payload,
    PaymentGateway Gateway) : ICommand;
