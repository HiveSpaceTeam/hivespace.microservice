namespace HiveSpace.PaymentService.Application.Wallets.Dtos;

public record TransactionDto(
    Guid Id,
    string Type,
    string Direction,
    long Amount,
    string Currency,
    long BalanceAfter,
    string Reference,
    string Description,
    DateTimeOffset TransactedAt);
