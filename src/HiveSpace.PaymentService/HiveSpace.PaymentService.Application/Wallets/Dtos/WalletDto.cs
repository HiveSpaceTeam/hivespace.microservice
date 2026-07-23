using HiveSpace.Application.Shared.Dtos;

namespace HiveSpace.PaymentService.Application.Wallets.Dtos;

public record WalletDto(
    Guid WalletId,
    Guid UserId,
    MoneyResponseDto AvailableBalance,
    MoneyResponseDto EscrowBalance,
    MoneyResponseDto TotalBalance,
    int RewardPoints,
    string Status);
