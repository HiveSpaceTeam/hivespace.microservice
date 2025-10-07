using HiveSpace.Domain.Shared.Exceptions;

namespace HiveSpace.CatalogService.Domain.Exceptions
{
    public class InvalidAttributeException : DomainException
    {
        public InvalidAttributeException()
      : base(422, CatalogErrorCode.InvalidAttribute, nameof(InvalidAttributeException))
        {
        }
    }
}
