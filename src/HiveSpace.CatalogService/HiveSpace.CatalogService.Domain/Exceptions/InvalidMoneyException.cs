using HiveSpace.Domain.Shared.Exceptions;

namespace HiveSpace.CatalogService.Domain.Exceptions;

public class InvalidMoneyException : DomainException
{
    public InvalidMoneyException()
        : base(422, CatalogErrorCode.InvalidMoney, nameof(InvalidMoneyException))
    {
    }
} 