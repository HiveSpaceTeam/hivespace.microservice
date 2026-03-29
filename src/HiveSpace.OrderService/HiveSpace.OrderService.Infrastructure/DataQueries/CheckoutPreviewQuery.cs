using Dapper;
using HiveSpace.OrderService.Application.Cart;
using HiveSpace.OrderService.Application.Cart.Dtos;
using Microsoft.Data.SqlClient;

namespace HiveSpace.OrderService.Infrastructure.DataQueries;

public class CheckoutPreviewQuery(string connectionString) : ICheckoutPreviewQuery
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
}
