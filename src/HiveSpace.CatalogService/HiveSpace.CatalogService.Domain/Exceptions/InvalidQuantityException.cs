using HiveSpace.Domain.Shared.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HiveSpace.CatalogService.Domain.Exceptions
{
    public class InvalidQuantityException : DomainException
    {
        public InvalidQuantityException()
            : base(400, CatalogErrorCode.InvalidQuantity, nameof(InvalidQuantityException))
        {
        }
    }

}
