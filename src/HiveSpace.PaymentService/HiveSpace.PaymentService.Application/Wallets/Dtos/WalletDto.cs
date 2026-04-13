namespace HiveSpace.PaymentService.Application.Wallets.Dtos;

public record WalletDto(
    Guid WalletId,
    Guid UserId,
    long AvailableBalance,
    string AvailableCurrency,
    long EscrowBalance,
    string EscrowCurrency,
    long TotalBalance,
    int RewardPoints,
    string Status);
