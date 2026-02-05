using Dapper;
using HiveSpace.Core.Models.Pagination;
using HiveSpace.UserService.Application.Constant.Enum;
using HiveSpace.UserService.Application.Interfaces.DataQueries;
using HiveSpace.UserService.Application.Models.Queries;
using HiveSpace.UserService.Application.Models.Responses.Admin;
using Microsoft.Data.SqlClient;

using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.Domain.Shared.Errors;

namespace HiveSpace.UserService.Infrastructure.DataQueries;

public class UnifiedUserDataQuery : IUnifiedUserDataQuery
{
    private readonly string _connectionString;

    public UnifiedUserDataQuery(string connectionString)
    {
        _connectionString = connectionString ?? throw new InvalidFieldException(DomainErrorCode.ArgumentNull, nameof(connectionString));
    }

    public async Task<PagedResult<UnifiedUserDto>> GetPagingUsersAsync(AdminUserFilterRequest request, UserQueryType queryType, CancellationToken cancellationToken = default)
    {
        var whereConditions = BuildWhereConditions(request, queryType);
        var orderBy = BuildOrderByClause(request.SortField, request.SortDirection);

        var mainQuery = $@"
            WITH FilteredUsers AS (
                SELECT DISTINCT
                    u.Id,
                    u.UserName AS Username,
                    u.FullName,
                    u.Email,
                    u.Status,
                    CAST(CASE WHEN u.StoreId IS NOT NULL THEN 1 ELSE 0 END AS BIT) AS IsSeller,
                    CAST(CASE WHEN u.RoleName = 'SystemAdmin' THEN 1 ELSE 0 END AS BIT) AS IsSystemAdmin,
                    CAST(u.CreatedAt AT TIME ZONE 'UTC' AS DATETIMEOFFSET) AS CreatedAt,
                    CAST(u.UpdatedAt AT TIME ZONE 'UTC' AS DATETIMEOFFSET) AS UpdatedAt,
                    CAST(u.LastLoginAt AT TIME ZONE 'UTC' AS DATETIMEOFFSET) AS LastLoginAt,
                    '' AS AvatarUrl
                FROM users u
                {GetRoleFilter(queryType)}
                {whereConditions}
            )
            SELECT * FROM FilteredUsers
            {orderBy}
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

        var countQuery = $@"
            SELECT COUNT(DISTINCT u.Id)
            FROM users u
            {GetRoleFilter(queryType)}
            {whereConditions}";

        var parameters = BuildParameters(request);
        var batchSql = mainQuery + ";" + countQuery + ";";

        using var connection = new SqlConnection(_connectionString);
        var cmd = new CommandDefinition(batchSql, parameters, commandTimeout: 30, cancellationToken: cancellationToken);
        using var grid = await connection.QueryMultipleAsync(cmd);        var items = (await grid.ReadAsync<UnifiedUserDto>()).AsList();
        var total = await grid.ReadSingleAsync<int>();

        return new PagedResult<UnifiedUserDto>(items, request.Page, request.PageSize, total);
    }

    public async Task<int> GetTotalUsersCountAsync(AdminUserFilterRequest request, UserQueryType queryType, CancellationToken cancellationToken = default)
    {
        using var connection = new SqlConnection(_connectionString);
        var whereConditions = BuildWhereConditions(request, queryType);
        var countQuery = $@"
            SELECT COUNT(DISTINCT u.Id)
            FROM users u
            {GetRoleFilter(queryType)}
            {whereConditions}";

        var parameters = BuildParameters(request);
        var cmd = new CommandDefinition(countQuery, parameters, commandTimeout: 30, cancellationToken: cancellationToken);
        return await connection.QuerySingleAsync<int>(cmd);
    }
    private static string GetRoleFilter(UserQueryType queryType)
    {
        return queryType switch
        {
            UserQueryType.Users => "WHERE (u.RoleName NOT IN ('SystemAdmin', 'Admin') OR u.RoleName IS NULL)",
            UserQueryType.Admins => "WHERE u.RoleName IN ('Admin', 'SystemAdmin')",
            _ => throw new InvalidFieldException(DomainErrorCode.InvalidEnumerationValue, nameof(queryType))
        };
    }

    private static string BuildWhereConditions(AdminUserFilterRequest request, UserQueryType queryType)
    {
        var conditions = new List<string>();

        // Status filter
        if (request.Status != StatusFilter.All)
        {
            conditions.Add("u.Status = @Status");
        }

        // Role filter based on query type
        if (request.Role != RoleFilter.All)
        {
            switch (queryType)
            {
                case UserQueryType.Users:
                    if (request.Role == RoleFilter.Seller)
                    {
                        conditions.Add("u.RoleName = 'Seller'");
                    }
                    else if (request.Role == RoleFilter.Customer)
                    {
                        conditions.Add("(u.RoleName IS NULL OR u.RoleName NOT IN ('Seller', 'Admin', 'SystemAdmin'))");
                    }
                    break;
                
                case UserQueryType.Admins:
                    if (request.Role == RoleFilter.RegularAdmin)
                    {
                        conditions.Add("u.RoleName = 'Admin'");
                    }
                    else if (request.Role == RoleFilter.SystemAdmin)
                    {
                        conditions.Add("u.RoleName = 'SystemAdmin'");
                    }
                    break;
            }
        }

        // Search filter (email)
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            conditions.Add("u.Email LIKE @SearchTerm");
        }

        conditions.Add("u.IsDeleted = 0");

        return conditions.Count > 0 ? $"AND {string.Join(" AND ", conditions)}" : string.Empty;
    }

    private static string BuildOrderByClause(string? field, string? direction)
    {
        var normalizedField = (field ?? "createdat").ToLowerInvariant();
        var column = normalizedField switch
        {
            "username" => "Username",
            "fullname" => "FullName",
            "email" => "Email",
            "status" => "Status",
            "lastlogindate" or "lastloginat" => "LastLoginAt",
            "createdat" or _ => "CreatedAt"
        };

        var dir = string.Equals(direction, "asc", StringComparison.OrdinalIgnoreCase) ? "ASC" : "DESC";
        return $"ORDER BY {column} {dir}, Id ASC";
    }

    private static object BuildParameters(AdminUserFilterRequest request)
    {
        return new
        {
            Offset = (request.Page - 1) * request.PageSize,
            PageSize = request.PageSize,
            Status = (int)request.Status,
            SearchTerm = !string.IsNullOrWhiteSpace(request.SearchTerm) ? $"%{request.SearchTerm.Trim()}%" : null
        };
    }
}