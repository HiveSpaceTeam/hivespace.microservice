using HiveSpace.Application.Shared.Queries;
using HiveSpace.Core.Models.Pagination;
using HiveSpace.PaymentService.Application.Wallets.Dtos;

namespace HiveSpace.PaymentService.Application.Wallets.Queries.GetTransactionHistory;

public record GetTransactionHistoryQuery(
    Guid UserId,
    int Page = 1,
    int PageSize = 20) : IQuery<PagedResult<TransactionDto>>;
