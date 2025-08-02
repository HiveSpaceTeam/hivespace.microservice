using HiveSpace.Domain.Shared.Errors;

namespace HiveSpace.CatalogService.Domain.Exceptions;
public class CatalogErrorCode(int id, string name, string code) : DomainErrorCode(id, name, code)
{
    public static readonly CatalogErrorCode InvalidSku = new(1, "InvalidSku", "CTL0001");
    public static readonly CatalogErrorCode InvalidMoney = new(2, "InvalidMoney", "CTL0002");
    public static readonly CatalogErrorCode InvalidAttribute = new(3, "InvalidAttribute", "CTL0003");
}
