using HiveSpace.Application.Shared.Dtos;

namespace HiveSpace.PaymentService.Application.Wallets.Dtos;

public record TransactionDto(
    Guid Id,
    string Type,
    string Direction,
    MoneyResponseDto Amount,
    MoneyResponseDto BalanceAfter,
    string Reference,
    string Description,
    DateTimeOffset TransactedAt);
