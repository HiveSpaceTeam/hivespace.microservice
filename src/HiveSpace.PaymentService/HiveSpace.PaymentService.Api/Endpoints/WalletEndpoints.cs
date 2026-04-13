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
            if (page < 1 || pageSize < 1 || pageSize > 100)
                return Results.BadRequest(new
                {
                    errors = new
                    {
                        page = page < 1 ? new[] { "page must be >= 1" } : null,
                        pageSize = pageSize < 1 || pageSize > 100 ? new[] { "pageSize must be between 1 and 100" } : null
                    }
                });

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
