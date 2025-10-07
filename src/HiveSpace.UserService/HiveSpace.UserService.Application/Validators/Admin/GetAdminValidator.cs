using FluentValidation;
using HiveSpace.Core.Exceptions;
using HiveSpace.Core.Exceptions.Models;
using HiveSpace.UserService.Application.Models.Requests.Admin;

namespace HiveSpace.UserService.Application.Validators.Admin;

public class GetAdminValidator : AbstractValidator<GetAdminRequestDto>
{
    private static readonly string[] ValidAdminSortFields = 
    {
        "fullname", "email", "status", "createdat", "lastupdatedat", "lastloginat"
    };

    public GetAdminValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThan(0)
            .WithState(_ => new Error(CommonErrorCode.InvalidPageNumber, nameof(GetAdminRequestDto.Page)));

        RuleFor(x => x.PageSize)
            .InclusiveBetween(10, 50)
            .WithState(_ => new Error(CommonErrorCode.InvalidPageSize, nameof(GetAdminRequestDto.PageSize)));

        RuleFor(x => x.Sort)
            .Must(sort => GetUsersValidationHelper.BeValidSortFormat(sort, ValidAdminSortFields))
            .When(x => !string.IsNullOrWhiteSpace(x.Sort))
            .WithState(_ => new Error(CommonErrorCode.InvalidSortFormat, nameof(GetAdminRequestDto.Sort)));
    }
}
