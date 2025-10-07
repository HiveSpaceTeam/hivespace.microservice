namespace HiveSpace.UserService.Application.Validators.Admin;

public static class GetUsersValidationHelper
{
    public static bool BeValidSortFormat(string? sort, string[] validFields)
    {
        if (string.IsNullOrWhiteSpace(sort)) return false;

        var parts = sort.Split('.');
        if (parts.Length != 2) return false;

        var validDirections = new[] { "asc", "desc" };

        return validFields.Contains(parts[0].ToLowerInvariant()) &&
               validDirections.Contains(parts[1].ToLowerInvariant());
    }
}