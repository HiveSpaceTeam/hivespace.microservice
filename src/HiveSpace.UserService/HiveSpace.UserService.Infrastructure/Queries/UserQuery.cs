using Dapper;
using HiveSpace.Core.Models.Pagination;
using HiveSpace.UserService.Application.Constant.Enum;
using HiveSpace.UserService.Application.Models.Queries;
using HiveSpace.UserService.Application.Models.Responses.Admin;
using HiveSpace.UserService.Application.Interfaces.DataQueries;
using Microsoft.Data.SqlClient;

namespace HiveSpace.UserService.Infrastructure.Queries;

public class UserQuery : IUserQuery
{
    private readonly string _connectionString;

    public UserQuery(string connectionString)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    }

    public async Task<PagedResult<UserListItemDto>> GetPagingUsersAsync(AdminUserFilterRequest request, CancellationToken cancellationToken = default)
    {
        var whereConditions = BuildWhereConditions(request);
        var orderBy = BuildOrderByClause(request.SortField, request.SortDirection);

        // Build main query
        var mainQuery = $@"
            SELECT 
                u.Id,
                u.UserName AS Username,
                u.FullName,
                u.Email,
                CASE 
                    WHEN u.UserStatus = 'Active' THEN 1
                    WHEN u.UserStatus = 'Inactive' THEN 2
                    ELSE CAST(u.UserStatus AS INT)
                END AS Status,
                CAST(CASE WHEN u.StoreId IS NOT NULL THEN 1 ELSE 0 END AS BIT) AS IsSeller,
                CAST(u.CreatedAt AS DATETIME) AS CreatedDate,
                CAST(u.LastLoginAt AS DATETIME) AS LastLoginDate,
                '' AS Avatar
            FROM users u
            {whereConditions}
            {orderBy}
            OFFSET @Offset ROWS
            FETCH NEXT @PageSize ROWS ONLY";

        // Build count query
        var countQuery = $@"
            SELECT COUNT(*)
            FROM users u
            {whereConditions}";

        var parameters = BuildParameters(request);

        // Execute queries in parallel using separate connections
        var usersTask = Task.Run(async () =>
        {
            using var connection = new SqlConnection(_connectionString);
            return await connection.QueryAsync<UserListItemDto>(mainQuery, parameters);
        });

        var countTask = Task.Run(async () =>
        {
            using var connection = new SqlConnection(_connectionString);
            return await connection.QuerySingleAsync<int>(countQuery, parameters);
        });

        await Task.WhenAll(usersTask, countTask);

        var users = await usersTask;
        var totalItems = await countTask;

        return new PagedResult<UserListItemDto>(
            users.ToList(),
            request.Page,
            request.PageSize,
            totalItems);
    }

    public async Task<int> GetTotalUsersCountAsync(AdminUserFilterRequest request, CancellationToken cancellationToken = default)
    {
        using var connection = new SqlConnection(_connectionString);

        var whereConditions = BuildWhereConditions(request);
        var countQuery = $@"
            SELECT COUNT(*)
            FROM users u
            {whereConditions}";

        var parameters = BuildParameters(request);
        return await connection.QuerySingleAsync<int>(countQuery, parameters);
    }

    private static string BuildWhereConditions(AdminUserFilterRequest request)
    {
        var conditions = new List<string>();

        // Status filter
        if (request.Status != UserStatusFilter.All)
        {
            conditions.Add("u.UserStatus = @StatusString");
        }

        // Role filter (Seller vs Customer)
        if (request.Role != UserRoleFilter.All)
        {
            if (request.Role == UserRoleFilter.Seller)
            {
                conditions.Add("u.StoreId IS NOT NULL");
            }
            else if (request.Role == UserRoleFilter.Customer)
            {
                conditions.Add("u.StoreId IS NULL");
            }
        }

        // Search filter (email)
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            conditions.Add("u.Email LIKE @SearchTerm");
        }

        return conditions.Count > 0 ? $"WHERE {string.Join(" AND ", conditions)}" : "";
    }

    private static string BuildOrderByClause(string field, string direction)
    {
        var column = field.ToLowerInvariant() switch
        {
            "username" => "u.UserName",
            "fullname" => "u.FullName",
            "email" => "u.Email",
            "status" => "u.UserStatus",
            "lastlogindate" => "u.LastLoginAt",
            "createddate" or _ => "u.CreatedAt"
        };

        var dir = direction.ToLowerInvariant() == "asc" ? "ASC" : "DESC";
        return $"ORDER BY {column} {dir}";
    }

    private static object BuildParameters(AdminUserFilterRequest request)
    {
        var statusString = request.Status switch
        {
            UserStatusFilter.Active => "Active",
            UserStatusFilter.Inactive => "Inactive",
            _ => null
        };

        var parameters = new
        {
            Offset = (request.Page - 1) * request.PageSize,
            PageSize = request.PageSize,
            StatusString = statusString,
            SearchTerm = !string.IsNullOrWhiteSpace(request.SearchTerm) ? $"%{request.SearchTerm.Trim()}%" : null
        };

        return parameters;
    }
}
