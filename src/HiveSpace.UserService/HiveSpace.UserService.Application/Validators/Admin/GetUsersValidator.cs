using FluentValidation;
using HiveSpace.Core.Exceptions;
using HiveSpace.Core.Exceptions.Models;
using HiveSpace.UserService.Application.Models.Requests.Admin;

namespace HiveSpace.UserService.Application.Validators.Admin;

public class GetUsersValidator : AbstractValidator<GetUsersRequestDto>
{
    private static readonly string[] ValidUserSortFields = 
    {
        "username", "fullname", "email", "status", "createdat", "updatedat", "lastloginat"
    };

    public GetUsersValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThan(0)
            .WithState(_ => new Error(CommonErrorCode.InvalidPageNumber, nameof(GetUsersRequestDto.Page)));

        RuleFor(x => x.PageSize)
            .InclusiveBetween(10, 50)
            .WithState(_ => new Error(CommonErrorCode.InvalidPageSize, nameof(GetUsersRequestDto.PageSize)));

        RuleFor(x => x.Sort)
            .Must(sort => GetUsersValidationHelper.BeValidSortFormat(sort, ValidUserSortFields))
            .When(x => !string.IsNullOrWhiteSpace(x.Sort))
            .WithState(_ => new Error(CommonErrorCode.InvalidSortFormat, nameof(GetUsersRequestDto.Sort)));
    }
}
