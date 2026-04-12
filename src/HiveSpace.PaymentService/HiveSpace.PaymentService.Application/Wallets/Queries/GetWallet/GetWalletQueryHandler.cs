using HiveSpace.Application.Shared.Handlers;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.PaymentService.Application.Wallets.Dtos;
using HiveSpace.PaymentService.Domain.Aggregates.Wallets;
using HiveSpace.PaymentService.Domain.Exceptions;
using HiveSpace.PaymentService.Domain.Repositories;

namespace HiveSpace.PaymentService.Application.Wallets.Queries.GetWallet;

public class GetWalletQueryHandler(IWalletRepository walletRepository)
    : IQueryHandler<GetWalletQuery, WalletDto>
{
    public async Task<WalletDto> Handle(GetWalletQuery request, CancellationToken cancellationToken)
    {
        var wallet = await walletRepository.GetByUserIdAsync(request.UserId, cancellationToken)
            ?? throw new NotFoundException(PaymentDomainErrorCode.WalletNotFound, nameof(Wallet));

        return new WalletDto(
            wallet.Id,
            wallet.UserId,
            wallet.AvailableBalance.Amount,
            wallet.AvailableBalance.Currency.ToString(),
            wallet.EscrowBalance.Amount,
            wallet.EscrowBalance.Currency.ToString(),
            wallet.TotalBalance.Amount,
            wallet.RewardPoints,
            wallet.Status.ToString());
    }
}
