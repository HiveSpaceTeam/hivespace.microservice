using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.PaymentService.Application.Payments.Commands.ProcessPaymentWebhook;
using HiveSpace.PaymentService.Application.Payments.Queries.GetPayment;
using HiveSpace.PaymentService.Application.Payments.Queries.GetPaymentByOrderId;
using HiveSpace.PaymentService.Domain.Aggregates.Payments.Enumerations;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace HiveSpace.PaymentService.Api.Endpoints;

public static class PaymentEndpoints
{
    public static IEndpointRouteBuilder MapPaymentEndpoints(this IEndpointRouteBuilder app)
    {
        // ReturnUrl — browser redirect from VNPay after the user completes (or cancels) payment.
        // Processes the payment result then redirects the user to the storefront result page.
        app.MapGet("/api/v1/payments/vnpay/return", async (
            HttpRequest request,
            ISender sender,
            IConfiguration config,
            CancellationToken ct) =>
        {
            var frontendUrl = config["FrontendUrl"]!;
            var payload = request.Query.ToDictionary(kv => kv.Key, kv => kv.Value.ToString());

            // vnp_TxnRef = payment.Id as GUID without dashes (set in VNPayGateway)
            if (!payload.TryGetValue("vnp_TxnRef", out var txnRef) ||
                !Guid.TryParseExact(txnRef, "N", out var paymentId))
                return Results.Redirect($"{frontendUrl}/payment/result?status=error");

            try
            {
                await sender.Send(new ProcessPaymentWebhookCommand(paymentId, payload, PaymentGateway.VNPay), ct);
            }
            catch
            {
                return Results.Redirect($"{frontendUrl}/payment/result?status=error");
            }

            try
            {
                var payment = await sender.Send(new GetPaymentQuery(paymentId), ct);
                var success = payload.TryGetValue("vnp_ResponseCode", out var code) && code == "00";
                var status = success ? "success" : "failed";
                return Results.Redirect($"{frontendUrl}/payment/result?orderId={payment.OrderId}&status={status}");
            }
            catch
            {
                return Results.Redirect($"{frontendUrl}/payment/result?status=error");
            }
        })
        .WithName("VnpayReturn")
        .WithTags("Payments");

        // Webhook — NO auth (called by VNPay via browser GET redirect or server IPN)
        app.MapGet("/api/v1/payments/webhook/{gateway}", async (
            string gateway,
            HttpRequest request,
            ISender sender,
            CancellationToken ct) =>
        {
            if (!Enum.TryParse<PaymentGateway>(gateway, ignoreCase: true, out var gatewayEnum))
                return Results.BadRequest($"Unknown gateway: {gateway}");

            var payload = request.Query.ToDictionary(kv => kv.Key, kv => kv.Value.ToString());

            // VNPay sends back vnp_TxnRef = payment.Id.ToString("N") (set by VNPayGateway)
            if (!payload.TryGetValue("vnp_TxnRef", out var txnRef) ||
                !Guid.TryParseExact(txnRef, "N", out var paymentId))
                return Results.Ok(new { RspCode = "01", Message = "Order Not Found" });

            // VNPay IPN requires a specific JSON response format.
            // Always return HTTP 200 — VNPay retries if it receives a non-200 or wrong format.
            try
            {
                await sender.Send(new ProcessPaymentWebhookCommand(paymentId, payload, gatewayEnum), ct);
                return Results.Ok(new { RspCode = "00", Message = "Confirm Success" });
            }
            catch (NotFoundException)
            {
                return Results.Ok(new { RspCode = "01", Message = "Order Not Found" });
            }
            catch (InvalidFieldException ex) when (ex.Message.Contains("vnp_SecureHash"))
            {
                return Results.Ok(new { RspCode = "97", Message = "Invalid Checksum" });
            }
            catch
            {
                return Results.Ok(new { RspCode = "99", Message = "Unknown error" });
            }
        })
        .WithName("PaymentWebhook")
        .WithTags("Payments");

        app.MapGet("/api/v1/payments/{paymentId:guid}", async (
            Guid paymentId,
            ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new GetPaymentQuery(paymentId), ct);
            return Results.Ok(result);
        })
        .RequireAuthorization()
        .WithName("GetPayment")
        .WithTags("Payments");

        app.MapGet("/api/v1/payments/by-order/{orderId:guid}", async (
            Guid orderId,
            ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new GetPaymentByOrderIdQuery(orderId), ct);
            return Results.Ok(result);
        })
        .RequireAuthorization()
        .WithName("GetPaymentByOrder")
        .WithTags("Payments");

        return app;
    }
}
