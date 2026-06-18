using FluentValidation;
using HiveSpace.Core.Exceptions;
using HiveSpace.Core.Exceptions.Models;
using HiveSpace.UserService.Application.Stores.Dtos;
using HiveSpace.UserService.Domain.Exceptions;

namespace HiveSpace.UserService.Application.Stores.Commands.CreateStore;

public class CreateStoreCommandValidator : AbstractValidator<CreateStoreCommand>
{
    public CreateStoreCommandValidator()
    {
        RuleFor(x => x.Payload.StoreName)
            .NotEmpty()
            .WithState(_ => new Error(CommonErrorCode.Required, nameof(CreateStoreRequestDto.StoreName)))
            .Length(2, 100)
            .WithState(_ => new Error(UserDomainErrorCode.InvalidField, nameof(CreateStoreRequestDto.StoreName)));

        RuleFor(x => x.Payload.StoreLogoFileId)
            .NotEmpty()
            .WithState(_ => new Error(CommonErrorCode.Required, nameof(CreateStoreRequestDto.StoreLogoFileId)))
            .MaximumLength(500)
            .WithState(_ => new Error(UserDomainErrorCode.InvalidField, nameof(CreateStoreRequestDto.StoreLogoFileId)));

        RuleFor(x => x.Payload.Address)
            .NotEmpty()
            .WithState(_ => new Error(CommonErrorCode.Required, nameof(CreateStoreRequestDto.Address)))
            .MaximumLength(500)
            .WithState(_ => new Error(UserDomainErrorCode.InvalidField, nameof(CreateStoreRequestDto.Address)));

        When(x => x.Payload.Description != null, () =>
        {
            RuleFor(x => x.Payload.Description)
                .MaximumLength(500)
                .WithState(_ => new Error(UserDomainErrorCode.InvalidField, nameof(CreateStoreRequestDto.Description)));
        });
    }
}
