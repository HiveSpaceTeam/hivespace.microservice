using Dapper;
using HiveSpace.Core.Models.Pagination;
using HiveSpace.UserService.Application.Constant.Enum;
using HiveSpace.UserService.Application.Models.Queries;
using HiveSpace.UserService.Application.Models.Responses.Admin;
using HiveSpace.UserService.Application.Interfaces;
using Microsoft.Data.SqlClient;

namespace HiveSpace.UserService.Infrastructure.DataQueries;

public class UserDataQuery : IUserDataQuery
{
    private readonly string _connectionString;

    public UserDataQuery(string connectionString)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    }

    public async Task<PagedResult<UserListItemDto>> GetPagingUsersAsync(AdminUserFilterRequest request, CancellationToken cancellationToken = default)
    {
        var whereConditions = BuildWhereConditions(request);
        var orderBy = BuildOrderByClause(request.SortField, request.SortDirection);

        // Build main query
        var mainQuery = $@"
            WITH FilteredUsers AS (
                SELECT DISTINCT
                    u.Id,
                    u.UserName AS Username,
                    u.FullName,
                    u.Email,
                    u.Status,
                    CAST(CASE WHEN u.StoreId IS NOT NULL THEN 1 ELSE 0 END AS BIT) AS IsSeller,
                    CAST(u.CreatedAt AT TIME ZONE 'UTC' AS DATETIME2) AS CreatedDate,
                    CAST(u.LastLoginAt AT TIME ZONE 'UTC' AS DATETIME2) AS LastLoginDate,
                    '' AS Avatar
                FROM users u
                WHERE NOT EXISTS (
                    SELECT 1 
                    FROM user_roles ur2 
                    JOIN roles r2 ON ur2.RoleId = r2.Id 
                    WHERE ur2.UserId = u.Id 
                    AND r2.Name IN ('SystemAdmin', 'Admin')
                )
                {whereConditions}
            )
            SELECT *
            FROM FilteredUsers
            {orderBy}
            OFFSET @Offset ROWS
            FETCH NEXT @PageSize ROWS ONLY";

        // Build count query
        var countQuery = $@"
            SELECT COUNT(DISTINCT u.Id)
            FROM users u
            WHERE NOT EXISTS (
                SELECT 1 
                FROM user_roles ur2 
                JOIN roles r2 ON ur2.RoleId = r2.Id 
                WHERE ur2.UserId = u.Id 
                AND r2.Name IN ('SystemAdmin', 'Admin')
            )
            {whereConditions}";

        var parameters = BuildParameters(request);

        // Execute queries in parallel using separate connections
        var batchSql = $"{mainQuery};{countQuery};";
        using var connection = new SqlConnection(_connectionString);
        var cmd = new CommandDefinition(batchSql, parameters, cancellationToken: cancellationToken);
        using var grid = await connection.QueryMultipleAsync(cmd);
        var users = (await grid.ReadAsync<UserListItemDto>()).AsList();
        var totalItems = await grid.ReadSingleAsync<int>();

        return new PagedResult<UserListItemDto>(
            users,
            request.Page,
            request.PageSize,
            totalItems);
    }

    public async Task<int> GetTotalUsersCountAsync(AdminUserFilterRequest request, CancellationToken cancellationToken = default)
    {
        using var connection = new SqlConnection(_connectionString);

        var whereConditions = BuildWhereConditions(request);  
        var countQuery = $@"  
            SELECT COUNT(DISTINCT u.Id)  
            FROM users u  
            WHERE NOT EXISTS (  
                SELECT 1 
                FROM user_roles ur2 
                JOIN roles r2 ON ur2.RoleId = r2.Id 
                WHERE ur2.UserId = u.Id 
                AND r2.Name IN ('SystemAdmin', 'Admin')  
            )  
            {whereConditions}";  

        var parameters = BuildParameters(request);  
        var cmd = new CommandDefinition(countQuery, parameters, cancellationToken: cancellationToken);  
        return await connection.QuerySingleAsync<int>(cmd);    
    }

    private static string BuildWhereConditions(AdminUserFilterRequest request)
    {
        var conditions = new List<string>();
        
        // Base condition to exclude SystemAdmin and Admin roles is already in the main query
        
        // Status filter
        if (request.Status != UserStatusFilter.All)
        {
            conditions.Add("u.Status = @Status");
        }

        // Role filter (Seller vs Customer)
        if (request.Role != UserRoleFilter.All)
        {
            if (request.Role == UserRoleFilter.Seller)
            {
                conditions.Add("EXISTS (SELECT 1 FROM user_roles ur3 JOIN roles r3 ON ur3.RoleId = r3.Id WHERE ur3.UserId = u.Id AND r3.Name = 'Seller')");
            }
            else if (request.Role == UserRoleFilter.Customer)
            {
                conditions.Add("NOT EXISTS (SELECT 1 FROM user_roles ur3 JOIN roles r3 ON ur3.RoleId = r3.Id WHERE ur3.UserId = u.Id AND r3.Name = 'Seller')");
            }
        }

        // Search filter (email)
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            conditions.Add("u.Email LIKE @SearchTerm");
        }

        return conditions.Count > 0 ? $"AND {string.Join(" AND ", conditions)}" : "";
    }

    private static string BuildOrderByClause(string field, string direction)
    {
        var column = field.ToLowerInvariant() switch
        {
            "username" => "Username",
            "fullname" => "FullName",
            "email" => "Email",
            "status" => "Status",
            "lastlogindate" => "LastLoginDate",
            "createddate" or _ => "CreatedDate"
        };

        var dir = direction.ToLowerInvariant() == "asc" ? "ASC" : "DESC";
        return $"ORDER BY {column} {dir}";
    }

    private static object BuildParameters(AdminUserFilterRequest request)
    {
        var parameters = new
        {
            Offset = (request.Page - 1) * request.PageSize,
            PageSize = request.PageSize,
            Status = (int)request.Status,
            SearchTerm = !string.IsNullOrWhiteSpace(request.SearchTerm) ? $"%{request.SearchTerm.Trim()}%" : null
        };

        return parameters;
    }
}
