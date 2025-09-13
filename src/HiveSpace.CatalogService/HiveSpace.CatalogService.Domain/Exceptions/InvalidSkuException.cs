using HiveSpace.Domain.Shared.Exceptions;

namespace HiveSpace.CatalogService.Domain.Exceptions;

public class InvalidSkuException : DomainException
{
    public InvalidSkuException()
        : base(422, CatalogErrorCode.InvalidSku, nameof(InvalidSkuException))
    {
    }
} 