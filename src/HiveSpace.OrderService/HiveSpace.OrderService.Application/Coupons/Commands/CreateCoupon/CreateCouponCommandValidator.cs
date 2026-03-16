using HiveSpace.Core.Exceptions;
using HiveSpace.Core.Exceptions.Models;
using HiveSpace.OrderService.Domain.Exceptions;
using FluentValidation;

namespace HiveSpace.OrderService.Application.Coupons.Commands.CreateCoupon;

public class CreateCouponCommandValidator : AbstractValidator<CreateCouponCommand>
{
    public CreateCouponCommandValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty()
            .WithState(_ => new Error(CommonErrorCode.Required, nameof(CreateCouponCommand.Code)))
            .MaximumLength(10)
            .WithState(_ => new Error(OrderDomainErrorCode.CouponCodeInvalidLength, nameof(CreateCouponCommand.Code)));

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithState(_ => new Error(CommonErrorCode.Required, nameof(CreateCouponCommand.Name)))
            .MaximumLength(100)
            .WithState(_ => new Error(OrderDomainErrorCode.CouponNameInvalidLength, nameof(CreateCouponCommand.Name)));
        
        RuleFor(x => x.StartDateTime)
            .NotEmpty()
            .WithState(_ => new Error(CommonErrorCode.Required, nameof(CreateCouponCommand.StartDateTime)))
            .Must(dt => dt > DateTimeOffset.UtcNow)
            .WithState(_ => new Error(OrderDomainErrorCode.CouponInvalidDates, nameof(CreateCouponCommand.StartDateTime)));
            
        RuleFor(x => x.EndDateTime)
            .NotEmpty()
            .WithState(_ => new Error(CommonErrorCode.Required, nameof(CreateCouponCommand.EndDateTime)))
            .GreaterThan(x => x.StartDateTime)
            .WithState(_ => new Error(OrderDomainErrorCode.CouponInvalidDates, nameof(CreateCouponCommand.EndDateTime)));

        RuleFor(x => x.DiscountType).IsInEnum()
            .WithState(_ => new Error(OrderDomainErrorCode.CouponInvalid, nameof(CreateCouponCommand.DiscountType)));
        
        RuleFor(x => x.Scope).IsInEnum()
            .WithState(_ => new Error(OrderDomainErrorCode.CouponInvalid, nameof(CreateCouponCommand.Scope)));
        
        When(x => x.DiscountType == HiveSpace.OrderService.Domain.Enumerations.DiscountType.FixedAmount, () =>
        {
            RuleFor(x => x.DiscountAmount).GreaterThan(0)
                .WithState(_ => new Error(OrderDomainErrorCode.CouponInvalidDiscountAmount, nameof(CreateCouponCommand.DiscountAmount)));
        });
        RuleFor(x => x.DiscountCurrency).NotEmpty()
            .WithState(_ => new Error(CommonErrorCode.Required, nameof(CreateCouponCommand.DiscountCurrency)));
        
        When(x => x.DiscountType == HiveSpace.OrderService.Domain.Enumerations.DiscountType.Percentage, () =>
        {
            RuleFor(x => x.DiscountPercentage)
                .NotNull()
                .WithState(_ => new Error(CommonErrorCode.Required, nameof(CreateCouponCommand.DiscountPercentage)))
                .GreaterThan(0)
                .WithState(_ => new Error(OrderDomainErrorCode.CouponInvalidPercentage, nameof(CreateCouponCommand.DiscountPercentage)))
                .LessThanOrEqualTo(100)
                .WithState(_ => new Error(OrderDomainErrorCode.CouponInvalidPercentage, nameof(CreateCouponCommand.DiscountPercentage)));

            When(x => x.MaxDiscountAmount.HasValue, () =>
            {
                RuleFor(x => x.MaxDiscountAmount)
                    .GreaterThanOrEqualTo(x => (long?)(x.MinOrderAmount * (x.DiscountPercentage / 100m)))
                    .WithState(_ => new Error(OrderDomainErrorCode.CouponMaxDiscountTooSmall, nameof(CreateCouponCommand.MinOrderAmount)));
            });
        });

        RuleFor(x => x.MinOrderAmount)
            .GreaterThan(0)
            .WithState(_ => new Error(OrderDomainErrorCode.CouponInvalidMinOrderAmount, nameof(CreateCouponCommand.MinOrderAmount)));

        RuleFor(x => x.MaxUsageCount)
            .GreaterThan(0)
            .WithState(_ => new Error(OrderDomainErrorCode.CouponInvalidUsageLimit, nameof(CreateCouponCommand.MaxUsageCount)));

        RuleFor(x => x.MaxUsagePerUser)
            .GreaterThan(0)
            .WithState(_ => new Error(OrderDomainErrorCode.CouponInvalidUsageLimitPerUser, nameof(CreateCouponCommand.MaxUsagePerUser)));
    }
}
