using HiveSpace.Application.Shared.Handlers;
using HiveSpace.Core.Models.Pagination;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.PaymentService.Application.Wallets.Dtos;
using HiveSpace.PaymentService.Domain.Aggregates.Wallets;
using HiveSpace.PaymentService.Domain.Exceptions;
using HiveSpace.PaymentService.Domain.Repositories;

namespace HiveSpace.PaymentService.Application.Wallets.Queries.GetTransactionHistory;

public class GetTransactionHistoryQueryHandler(IWalletRepository walletRepository)
    : IQueryHandler<GetTransactionHistoryQuery, PagedResult<TransactionDto>>
{
    public async Task<PagedResult<TransactionDto>> Handle(
        GetTransactionHistoryQuery request, CancellationToken cancellationToken)
    {
        var wallet = await walletRepository.GetByUserIdWithTransactionsAsync(request.UserId, cancellationToken)
            ?? throw new NotFoundException(PaymentDomainErrorCode.WalletNotFound, nameof(Wallet));

        var ordered = wallet.Transactions
            .OrderByDescending(t => t.TransactedAt)
            .ToList();

        var total = ordered.Count;
        var items = ordered
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(t => new TransactionDto(
                t.Id,
                t.Type.ToString(),
                t.Direction.ToString(),
                t.Amount.Amount,
                t.Amount.Currency.ToString(),
                t.BalanceAfter.Amount,
                t.Reference,
                t.Description,
                t.TransactedAt))
            .ToList();

        return new PagedResult<TransactionDto>(items, request.Page, request.PageSize, total);
    }
}
