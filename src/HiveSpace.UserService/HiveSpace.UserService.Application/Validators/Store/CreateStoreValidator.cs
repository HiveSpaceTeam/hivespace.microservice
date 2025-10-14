using FluentValidation;
using HiveSpace.Core;
using HiveSpace.Core.Exceptions;
using HiveSpace.Core.Exceptions.Models;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.UserService.Application.Models.Requests.Store;
using HiveSpace.UserService.Domain.Exceptions;

namespace HiveSpace.UserService.Application.Validators.Store;

public class CreateStoreValidator : AbstractValidator<CreateStoreRequestDto>
{
    public CreateStoreValidator()
    {
        RuleFor(x => x.StoreName)
            .NotEmpty()
            .WithState(_ => new Error(CommonErrorCode.Required, nameof(CreateStoreRequestDto.StoreName)))
            .Length(2, 100)
            .WithState(_ => new Error(UserDomainErrorCode.InvalidField, nameof(CreateStoreRequestDto.StoreName)));

        RuleFor(x => x.StoreLogoFileId)
            .NotEmpty()
            .WithState(_ => new Error(CommonErrorCode.Required, nameof(CreateStoreRequestDto.StoreLogoFileId)))
            .MaximumLength(500)
            .WithState(_ => new Error(UserDomainErrorCode.InvalidField, nameof(CreateStoreRequestDto.StoreLogoFileId)));
            
        RuleFor(x => x.Address)
            .NotEmpty()
            .WithState(_ => new Error(CommonErrorCode.Required, nameof(CreateStoreRequestDto.Address)))
            .MaximumLength(500)
            .WithState(_ => new Error(UserDomainErrorCode.InvalidField, nameof(CreateStoreRequestDto.Address)));
            
        When(x => x.Description != null, () =>
        {
            RuleFor(x => x.Description)
                .MaximumLength(500)
                .WithState(_ => new Error(UserDomainErrorCode.InvalidField, nameof(CreateStoreRequestDto.Description)));
        });
    }
}