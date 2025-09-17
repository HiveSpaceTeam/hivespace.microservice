using FluentValidation;
using HiveSpace.UserService.Application.Models.Requests.Admin;

namespace HiveSpace.UserService.Application.Validators.Admin;

public class GetUsersValidator : AbstractValidator<GetUsersRequestDto>
{
    public GetUsersValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThan(0)
            .WithMessage("Page must be greater than 0");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(10, 50)
            .WithMessage("Page size must be between 10 and 50");

        RuleFor(x => x.Role)
            .IsInEnum()
            .WithMessage("Invalid role filter value");

        RuleFor(x => x.Status)
            .IsInEnum()
            .WithMessage("Invalid status filter value");

        RuleFor(x => x.SearchTerm)
            .EmailAddress()
            .When(x => !string.IsNullOrWhiteSpace(x.SearchTerm))
            .WithMessage("Search term must be a valid email address");

        RuleFor(x => x.Sort)
            .Must(BeValidSortFormat)
            .WithMessage("Sort must be in format 'field.direction' (e.g., 'email.asc', 'createdDate.desc')");
    }

    private static bool BeValidSortFormat(string sort)
    {
        if (string.IsNullOrWhiteSpace(sort)) return false;

        var parts = sort.Split('.');
        if (parts.Length != 2) return false;

        var validFields = new[] { "username", "fullname", "email", "status", "createddate", "lastlogindate" };
        var validDirections = new[] { "asc", "desc" };

        return validFields.Contains(parts[0].ToLowerInvariant()) &&
               validDirections.Contains(parts[1].ToLowerInvariant());
    }
}
