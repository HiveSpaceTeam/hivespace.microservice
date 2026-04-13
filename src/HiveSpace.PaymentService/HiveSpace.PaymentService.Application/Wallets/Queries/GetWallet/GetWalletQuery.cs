using HiveSpace.Application.Shared.Queries;
using HiveSpace.PaymentService.Application.Wallets.Dtos;

namespace HiveSpace.PaymentService.Application.Wallets.Queries.GetWallet;

public record GetWalletQuery(Guid UserId) : IQuery<WalletDto>;
