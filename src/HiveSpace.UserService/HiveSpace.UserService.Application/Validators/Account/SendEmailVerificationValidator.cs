using FluentValidation;
using HiveSpace.UserService.Domain.Exceptions;
using HiveSpace.Core.Exceptions;
using HiveSpace.Core.Exceptions.Models;
using HiveSpace.UserService.Application.Models.Requests.Account;

namespace HiveSpace.UserService.Application.Validators.Account;

public class SendEmailVerificationValidator : AbstractValidator<SendEmailVerificationRequestDto>
{
    public SendEmailVerificationValidator()
    {
        RuleFor(x => x.CallbackUrl)
            .NotEmpty()
            .WithState(_ => new Error(CommonErrorCode.Required, nameof(SendEmailVerificationRequestDto.CallbackUrl)))
            .Must(BeValidUrl)
            .WithState(_ => new Error(UserDomainErrorCode.InvalidField, nameof(SendEmailVerificationRequestDto.CallbackUrl)));

        // RuleFor(x => x.ReturnUrl)
        //     .Must(BeValidUrl)
        //     .When(x => !string.IsNullOrWhiteSpace(x.ReturnUrl))
        //     .WithState(_ => new Error(UserDomainErrorCode.InvalidField, nameof(SendEmailVerificationRequestDto.ReturnUrl)));
    }

    private static bool BeValidUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url)) return true;
        return Uri.TryCreate(url, UriKind.Absolute, out var result) 
            && (result.Scheme == Uri.UriSchemeHttp || result.Scheme == Uri.UriSchemeHttps);
    }
}