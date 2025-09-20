using FluentValidation;
using HiveSpace.Core.Exceptions;
using HiveSpace.Core.Exceptions.Models;
using HiveSpace.UserService.Application.Models.Requests.Admin;
using HiveSpace.UserService.Domain.Exceptions;

namespace HiveSpace.UserService.Application.Validators.Admin;

public class CreateAdminValidator : AbstractValidator<CreateAdminRequestDto>
{
    public CreateAdminValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty()
            .WithState(_ => new Error(CommonErrorCode.Required, nameof(CreateAdminRequestDto.FullName)))
            .Length(2, 100)  
            .WithState(_ => new Error(UserDomainErrorCode.InvalidField, nameof(CreateAdminRequestDto.FullName)));

        RuleFor(x => x.Email)
            .NotEmpty()
            .WithState(_ => new Error(CommonErrorCode.Required, nameof(CreateAdminRequestDto.Email)))
            .EmailAddress()
            .WithState(_ => new Error(UserDomainErrorCode.InvalidEmail, nameof(CreateAdminRequestDto.Email)));

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithState(_ => new Error(CommonErrorCode.Required, nameof(CreateAdminRequestDto.Password)))
            .MinimumLength(12)
            .WithState(_ => new Error(UserDomainErrorCode.InvalidPassword, nameof(CreateAdminRequestDto.Password)))
            .Matches("^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d)(?=.*[@$!%*?&])[A-Za-z\\d@$!%*?&]+$")
            .WithState(_ => new Error(UserDomainErrorCode.InvalidPassword, nameof(CreateAdminRequestDto.Password)));
            
        RuleFor(x => x.ConfirmPassword)
            .NotEmpty()
            .WithState(_ => new Error(CommonErrorCode.Required, nameof(CreateAdminRequestDto.ConfirmPassword)))
            .Equal(x => x.Password)
            .WithState(_ => new Error(UserDomainErrorCode.PasswordMismatch, nameof(CreateAdminRequestDto.ConfirmPassword)));

        // Admin type validation - IsSystemAdmin boolean is automatically validated by the model binding
        // Business rule validation (regular admin cannot create system admin) should be handled in the domain service
    }
}
