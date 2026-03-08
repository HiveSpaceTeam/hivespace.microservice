using System;
using HiveSpace.Core.Exceptions;
using HiveSpace.Core.Exceptions.Models;
using HiveSpace.OrderService.Domain.Exceptions;
using FluentValidation;

namespace HiveSpace.OrderService.Application.Coupons.Commands.UpdateCoupon;

public class UpdateCouponCommandValidator : AbstractValidator<UpdateCouponCommand>
{
    public UpdateCouponCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithState(_ => new Error(CommonErrorCode.Required, nameof(UpdateCouponCommand.Id)));

        RuleFor(x => x.Code)
            .NotEmpty()
            .WithState(_ => new Error(CommonErrorCode.Required, nameof(UpdateCouponCommand.Code)))
            .MaximumLength(10)
            .WithState(_ => new Error(OrderDomainErrorCode.CouponCodeInvalidLength, nameof(UpdateCouponCommand.Code)));

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithState(_ => new Error(CommonErrorCode.Required, nameof(UpdateCouponCommand.Name)))
            .MaximumLength(100)
            .WithState(_ => new Error(OrderDomainErrorCode.CouponNameInvalidLength, nameof(UpdateCouponCommand.Name)));
        
        RuleFor(x => x.StartDateTime)
            .NotEmpty()
            .WithState(_ => new Error(CommonErrorCode.Required, nameof(UpdateCouponCommand.StartDateTime)));
            
        RuleFor(x => x.EndDateTime)
            .NotEmpty()
            .WithState(_ => new Error(CommonErrorCode.Required, nameof(UpdateCouponCommand.EndDateTime)))
            .GreaterThan(x => x.StartDateTime)
            .WithState(_ => new Error(OrderDomainErrorCode.CouponInvalidDates, nameof(UpdateCouponCommand.EndDateTime)));

        When(x => x.DiscountAmount.HasValue, () =>
        {
            RuleFor(x => x.DiscountAmount).GreaterThan(0)
                .WithState(_ => new Error(OrderDomainErrorCode.CouponInvalidDiscountAmount, nameof(UpdateCouponCommand.DiscountAmount)));
        });
        
        When(x => x.DiscountAmount.HasValue || x.DiscountPercentage.HasValue, () => 
        {
            RuleFor(x => x.DiscountCurrency).NotEmpty()
                .WithState(_ => new Error(CommonErrorCode.Required, nameof(UpdateCouponCommand.DiscountCurrency)));
        });

        When(x => x.DiscountPercentage.HasValue, () =>
        {
            RuleFor(x => x.DiscountPercentage)
                .GreaterThan(0)
                .WithState(_ => new Error(OrderDomainErrorCode.CouponInvalidPercentage, nameof(UpdateCouponCommand.DiscountPercentage)))
                .LessThanOrEqualTo(100)
                .WithState(_ => new Error(OrderDomainErrorCode.CouponInvalidPercentage, nameof(UpdateCouponCommand.DiscountPercentage)));

            When(x => x.MaxDiscountAmount.HasValue, () =>
            {
                RuleFor(x => x.MaxDiscountAmount)
                    .GreaterThanOrEqualTo(x => (long?)(x.MinOrderAmount * (x.DiscountPercentage!.Value / 100m)))
                    .WithState(_ => new Error(OrderDomainErrorCode.CouponMaxDiscountTooSmall, nameof(UpdateCouponCommand.MinOrderAmount)));
            });
        });

        RuleFor(x => x.MinOrderAmount)
            .GreaterThan(0)
            .WithState(_ => new Error(OrderDomainErrorCode.CouponInvalidMinOrderAmount, nameof(UpdateCouponCommand.MinOrderAmount)));

        RuleFor(x => x.MaxUsageCount)
            .GreaterThan(0)
            .WithState(_ => new Error(OrderDomainErrorCode.CouponInvalidUsageLimit, nameof(UpdateCouponCommand.MaxUsageCount)));
    }
}
