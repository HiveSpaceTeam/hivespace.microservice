using HiveSpace.Core.Contexts;
using HiveSpace.PaymentService.Application.Wallets.Queries.GetTransactionHistory;
using HiveSpace.PaymentService.Application.Wallets.Queries.GetWallet;
using MediatR;

namespace HiveSpace.PaymentService.Api.Endpoints;

public static class WalletEndpoints
{
    public static IEndpointRouteBuilder MapWalletEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/wallets/me", async (
            IUserContext userContext,
            ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new GetWalletQuery(userContext.UserId), ct);
            return Results.Ok(result);
        })
        .RequireAuthorization()
        .WithName("GetMyWallet")
        .WithTags("Wallets");

        app.MapGet("/api/v1/wallets/me/transactions", async (
            IUserContext userContext,
            ISender sender,
            int page = 1,
            int pageSize = 20,
            CancellationToken ct = default) =>
        {
            var result = await sender.Send(
                new GetTransactionHistoryQuery(userContext.UserId, page, pageSize), ct);
            return Results.Ok(result);
        })
        .RequireAuthorization()
        .WithName("GetMyTransactions")
        .WithTags("Wallets");

        return app;
    }
}
