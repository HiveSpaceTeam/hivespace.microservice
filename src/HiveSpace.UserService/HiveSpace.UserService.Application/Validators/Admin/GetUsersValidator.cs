using FluentValidation;
using HiveSpace.Core.Exceptions;
using HiveSpace.Core.Exceptions.Models;
using HiveSpace.UserService.Application.Models.Requests.Admin;

namespace HiveSpace.UserService.Application.Validators.Admin;

public class GetUsersValidator : AbstractValidator<GetUsersRequestDto>
{
    public GetUsersValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThan(0)
            .WithState(_ => new Error(CommonErrorCode.InvalidPageNumber, nameof(GetUsersRequestDto.Page)));

        RuleFor(x => x.PageSize)
            .InclusiveBetween(10, 50)
            .WithState(_ => new Error(CommonErrorCode.InvalidPageSize, nameof(GetUsersRequestDto.PageSize)));

        RuleFor(x => x.Role)
            .IsInEnum()
            .WithState(_ => new Error(CommonErrorCode.InvalidRoleFilter, nameof(GetUsersRequestDto.Role)));

        RuleFor(x => x.Status)
            .IsInEnum()
            .WithState(_ => new Error(CommonErrorCode.InvalidStatusFilter, nameof(GetUsersRequestDto.Status)));

        RuleFor(x => x.SearchTerm)
            .EmailAddress()
            .When(x => !string.IsNullOrWhiteSpace(x.SearchTerm))
            .WithState(_ => new Error(CommonErrorCode.InvalidSearchTerm, nameof(GetUsersRequestDto.SearchTerm)));

        RuleFor(x => x.Sort)
            .Must(BeValidSortFormat)
            .When(x => !string.IsNullOrWhiteSpace(x.Sort))
            .WithState(_ => new Error(CommonErrorCode.InvalidSortFormat, nameof(GetUsersRequestDto.Sort)));
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
