using Dapper;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.OrderService.Application.Cart;
using HiveSpace.OrderService.Application.Cart.Dtos;
using HiveSpace.OrderService.Application.Orders.Dtos;
using HiveSpace.OrderService.Domain.Exceptions;
using HiveSpace.OrderService.Infrastructure.Sagas;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace HiveSpace.OrderService.Infrastructure.DataQueries;

public class CheckoutDataQuery(string connectionString, IDbContextFactory<Data.OrderDbContext> dbFactory)
    : ICheckoutQuery
{
    public async Task<CheckoutPreviewRawResult> GetSelectedCartItemsAsync(
        Guid userId, CancellationToken ct = default)
    {
        const string sql = @"
            SELECT
                ci.Id               AS CartItemId,
                ci.ProductId,
                ci.SkuId,
                ci.Quantity,
                pr.Name             AS ProductName,
                pr.ThumbnailUrl,
                skr.Price,
                skr.Currency,
                skr.ImageUrl        AS SkuImageUrl,
                skr.Attributes      AS SkuAttributes,
                pr.StoreId,
                sr.Name             AS StoreName
            FROM carts c
            INNER JOIN cart_items ci   ON ci.CartId  = c.Id
            LEFT  JOIN product_refs pr ON pr.Id      = ci.ProductId
            LEFT  JOIN store_refs sr   ON sr.Id      = pr.StoreId
            LEFT  JOIN sku_refs skr    ON skr.Id     = ci.SkuId
            WHERE c.UserId = @UserId
              AND ci.IsSelected = 1;

            SELECT COUNT(*) FROM carts WHERE UserId = @UserId;";

        using var connection = new SqlConnection(connectionString);
        var cmd = new CommandDefinition(sql, new { UserId = userId },
                      commandTimeout: 30, cancellationToken: ct);
        using var grid = await connection.QueryMultipleAsync(cmd);

        var rows      = (await grid.ReadAsync<CheckoutPreviewRawRow>()).ToArray();
        var cartCount = await grid.ReadSingleAsync<int>();

        return new CheckoutPreviewRawResult(rows, CartExists: cartCount > 0);
    }

    public async Task<CheckoutStatusDto> GetCheckoutStatusAsync(
        Guid correlationId, CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);

        var saga = await db.Set<CheckoutSagaState>()
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.CorrelationId == correlationId, ct)
            ?? throw new NotFoundException(OrderDomainErrorCode.OrderNotFound, nameof(CheckoutSagaState));

        return new CheckoutStatusDto
        {
            CorrelationId = saga.CorrelationId,
            CurrentState  = saga.CurrentState,
            OrderIds      = saga.OrderStoreMap.Keys.ToList(),
            FailureReason = saga.FailureReason,
            IsCompleted   = saga.CurrentState == "Completed",
            IsFailed      = saga.CurrentState == "Failed"
        };
    }
}
