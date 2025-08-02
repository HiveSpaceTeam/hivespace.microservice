using HiveSpace.Domain.Shared.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
