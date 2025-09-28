  using Dapper;
using HiveSpace.Core.Models.Pagination;
using HiveSpace.UserService.Application.Constant.Enum;
using HiveSpace.UserService.Application.Interfaces.DataQueries;
using HiveSpace.UserService.Application.Models.Queries;
using HiveSpace.UserService.Application.Models.Responses.Admin;
using Microsoft.Data.SqlClient;

namespace HiveSpace.UserService.Infrastructure.DataQueries;

public class AdminDataQuery : IAdminDataQuery
{
    private readonly string _connectionString;

    public AdminDataQuery(string connectionString)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    }

    public async Task<PagedResult<AdminDto>> GetPagingAdminsAsync(AdminUserFilterRequest request, CancellationToken cancellationToken = default)
    {
        var whereConditions = BuildWhereConditions(request);
        var orderBy = BuildOrderByClause(request.SortField, request.SortDirection);

        var mainQuery = $@"
            WITH FilteredAdmins AS (
                SELECT DISTINCT
                    u.Id,
                    u.UserName AS Username,
                    u.FullName,
                    u.Email,
                    u.Status,
                    CAST(CASE WHEN EXISTS (SELECT 1 FROM user_roles ur2 JOIN roles r2 ON ur2.RoleId = r2.Id WHERE ur2.UserId = u.Id AND r2.Name = 'SystemAdmin') THEN 1 ELSE 0 END AS BIT) AS IsSystemAdmin,
                    CAST(u.CreatedAt AT TIME ZONE 'UTC' AS DATETIMEOFFSET) AS CreatedAt,
                    CAST(u.UpdatedAt AT TIME ZONE 'UTC' AS DATETIMEOFFSET) AS UpdatedAt,
                    CAST(u.LastLoginAt AT TIME ZONE 'UTC' AS DATETIMEOFFSET) AS LastLoginAt,
                    '' AS AvatarUrl
                FROM users u
                WHERE EXISTS (
                    SELECT 1 FROM user_roles ur2 JOIN roles r2 ON ur2.RoleId = r2.Id WHERE ur2.UserId = u.Id AND r2.Name IN ('Admin','SystemAdmin')
                )
                {whereConditions}
            )
            SELECT * FROM FilteredAdmins
            {orderBy}
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

        var countQuery = $@"
            SELECT COUNT(DISTINCT u.Id)
            FROM users u
            WHERE EXISTS (
                SELECT 1 FROM user_roles ur2 JOIN roles r2 ON ur2.RoleId = r2.Id WHERE ur2.UserId = u.Id AND r2.Name IN ('Admin','SystemAdmin')
            )
            {whereConditions}";

        var parameters = BuildParameters(request);
        var batchSql = mainQuery + ";" + countQuery + ";";

        using var connection = new SqlConnection(_connectionString);
        var cmd = new CommandDefinition(batchSql, parameters, cancellationToken: cancellationToken);
        using var grid = await connection.QueryMultipleAsync(cmd);
        var items = (await grid.ReadAsync<AdminDto>()).AsList();
        var total = await grid.ReadSingleAsync<int>();

        return new PagedResult<AdminDto>(items, request.Page, request.PageSize, total);
    }

    public async Task<int> GetTotalAdminsCountAsync(AdminUserFilterRequest request, CancellationToken cancellationToken = default)
    {
        using var connection = new SqlConnection(_connectionString);
        var whereConditions = BuildWhereConditions(request);
        var countQuery = $@"
            SELECT COUNT(DISTINCT u.Id)
            FROM users u
            WHERE EXISTS (
                SELECT 1 FROM user_roles ur2 JOIN roles r2 ON ur2.RoleId = r2.Id WHERE ur2.UserId = u.Id AND r2.Name IN ('Admin','SystemAdmin')
            )
            {whereConditions}";

        var parameters = BuildParameters(request);
        var cmd = new CommandDefinition(countQuery, parameters, cancellationToken: cancellationToken);
        return await connection.QuerySingleAsync<int>(cmd);
    }

    private static string BuildWhereConditions(AdminUserFilterRequest request)
    {
        var conditions = new List<string>();

        // Status filter
        if (request.Status != StatusFilter.All)
        {
            conditions.Add("u.Status = @Status");
        }

        // Role filter: only consider admin-related values here (RegularAdmin, SystemAdmin)
        if (request.Role != RoleFilter.All)
        {            
            if (request.Role == RoleFilter.RegularAdmin)
            {
                conditions.Add("EXISTS (SELECT 1 FROM user_roles ur3 JOIN roles r3 ON ur3.RoleId = r3.Id WHERE ur3.UserId = u.Id AND r3.Name IN ('Admin'))");
            }
            else if (request.Role == RoleFilter.SystemAdmin)
            {
                conditions.Add("EXISTS (SELECT 1 FROM user_roles ur3 JOIN roles r3 ON ur3.RoleId = r3.Id WHERE ur3.UserId = u.Id AND r3.Name = 'SystemAdmin')");
            }
        }

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            conditions.Add("u.Email LIKE @SearchTerm");
        }

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
            "lastlogindate" => "LastLoginAt",
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
