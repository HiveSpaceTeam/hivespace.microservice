using Dapper;
using HiveSpace.Core.Models.Pagination;
using HiveSpace.OrderService.Application.Cart;
using HiveSpace.OrderService.Application.Cart.Dtos;
using Microsoft.Data.SqlClient;

namespace HiveSpace.OrderService.Infrastructure.DataQueries;

public class CartDataQuery(string connectionString) : ICartDataQuery
{
    public async Task<PagedResult<CartItemDto>> GetPagedCartItemsAsync(
        Guid userId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var mainQuery = @"
            SELECT
                ci.Id               AS CartItemId,
                ci.ProductId,
                ci.SkuId,
                ci.Quantity,
                ci.IsSelected,
                pr.Name             AS ProductName,
                pr.ThumbnailUrl     AS ProductThumbnailUrl,
                pr.Status           AS ProductStatus,
                skr.Price,
                skr.Currency,
                skr.SkuNo,
                skr.SkuName,
                skr.ImageUrl        AS SkuImageUrl,
                skr.Attributes      AS SkuAttributes,
                sr.Name             AS StoreName,
                sr.Status           AS StoreStatus,
                ci.CreatedAt,
                ci.UpdatedAt
            FROM carts c
            INNER JOIN cart_items ci  ON ci.CartId    = c.Id
            LEFT JOIN  product_refs pr ON pr.Id       = ci.ProductId
            LEFT JOIN  store_refs sr  ON sr.Id        = pr.StoreId
            LEFT JOIN  sku_refs skr   ON skr.Id       = ci.SkuId
            WHERE c.UserId = @UserId
            ORDER BY ci.CreatedAt DESC
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

        var countQuery = @"
            SELECT COUNT(*)
            FROM carts c
            INNER JOIN cart_items ci ON ci.CartId = c.Id
            WHERE c.UserId = @UserId";

        var parameters = new
        {
            UserId = userId,
            Offset = (page - 1) * pageSize,
            PageSize = pageSize
        };

        using var connection = new SqlConnection(connectionString);
        var cmd = new CommandDefinition(mainQuery + ";" + countQuery + ";", parameters, commandTimeout: 30, cancellationToken: cancellationToken);
        using var grid = await connection.QueryMultipleAsync(cmd);

        var items = (await grid.ReadAsync<CartItemDto>()).AsList();
        var total = await grid.ReadSingleAsync<int>();

        return new PagedResult<CartItemDto>(items, page, pageSize, total);
    }
}
