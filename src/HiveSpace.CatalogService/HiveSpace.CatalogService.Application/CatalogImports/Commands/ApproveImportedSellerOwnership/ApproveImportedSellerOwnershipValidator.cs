using FluentValidation;
using HiveSpace.Core.Exceptions.Models;
using HiveSpace.Core.Exceptions;

namespace HiveSpace.CatalogService.Application.CatalogImports.Commands.ApproveImportedSellerOwnership;

public class ApproveImportedSellerOwnershipValidator : AbstractValidator<ApproveImportedSellerOwnershipCommand>
{
    public ApproveImportedSellerOwnershipValidator()
    {
        RuleFor(x => x.BundleId)
            .NotEmpty()
            .WithState(_ => new Error(CommonErrorCode.Required, nameof(ApproveImportedSellerOwnershipCommand.BundleId)));

        RuleFor(x => x.ImportedSellerId)
            .NotEmpty()
            .WithState(_ => new Error(CommonErrorCode.Required, nameof(ApproveImportedSellerOwnershipCommand.ImportedSellerId)));

        RuleFor(x => x.TargetUserId)
            .NotEmpty()
            .WithState(_ => new Error(CommonErrorCode.Required, nameof(ApproveImportedSellerOwnershipCommand.TargetUserId)));

        RuleFor(x => x.TargetStoreId)
            .NotEmpty()
            .WithState(_ => new Error(CommonErrorCode.Required, nameof(ApproveImportedSellerOwnershipCommand.TargetStoreId)));

        RuleFor(x => x.ApprovedByUserId)
            .NotEmpty()
            .WithState(_ => new Error(CommonErrorCode.Required, nameof(ApproveImportedSellerOwnershipCommand.ApprovedByUserId)));

        RuleFor(x => x.ApprovalReason)
            .NotEmpty()
            .WithState(_ => new Error(CommonErrorCode.Required, nameof(ApproveImportedSellerOwnershipCommand.ApprovalReason)));
    }
}
