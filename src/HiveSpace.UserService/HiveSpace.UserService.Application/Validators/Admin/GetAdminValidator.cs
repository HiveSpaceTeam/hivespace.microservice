using FluentValidation;
using HiveSpace.Core.Exceptions;
using HiveSpace.Core.Exceptions.Models;
using HiveSpace.UserService.Application.Models.Requests.Admin;
using HiveSpace.UserService.Application.Constant.Enum;

namespace HiveSpace.UserService.Application.Validators.Admin;

public class GetAdminValidator : AbstractValidator<GetAdminRequestDto>
{
    public GetAdminValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThan(0)
            .WithState(_ => new Error(CommonErrorCode.InvalidPageNumber, nameof(GetAdminRequestDto.Page)));

        RuleFor(x => x.PageSize)
            .InclusiveBetween(10, 50)
            .WithState(_ => new Error(CommonErrorCode.InvalidPageSize, nameof(GetAdminRequestDto.PageSize)));

        RuleFor(x => x.SearchTerm)
            .EmailAddress()
            .When(x => !string.IsNullOrWhiteSpace(x.SearchTerm))
            .WithState(_ => new Error(CommonErrorCode.InvalidSearchTerm, nameof(GetAdminRequestDto.SearchTerm)));

        RuleFor(x => x.Sort)
            .Must(BeValidSortFormat)
            .When(x => !string.IsNullOrWhiteSpace(x.Sort))
            .WithState(_ => new Error(CommonErrorCode.InvalidSortFormat, nameof(GetAdminRequestDto.Sort)));
    }

    private static bool BeValidSortFormat(string sort)
    {
        if (string.IsNullOrWhiteSpace(sort)) return false;

        var parts = sort.Split('.');
        if (parts.Length != 2) return false;

    var validFields = new[] { "fullname", "email", "status", "createdat", "lastupdatedat", "lastloginat" };
        var validDirections = new[] { "asc", "desc" };

        return validFields.Contains(parts[0].ToLowerInvariant()) &&
               validDirections.Contains(parts[1].ToLowerInvariant());
    }
}
