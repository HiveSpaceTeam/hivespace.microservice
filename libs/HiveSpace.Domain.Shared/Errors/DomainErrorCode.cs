using HiveSpace.Domain.Shared.Entities;

namespace HiveSpace.Domain.Shared.Errors;
public class DomainErrorCode(int id, string name, string code) : Enumeration(id, name), IErrorCode
{
    public string Code { get; private set; } = code;
    public string Name => base.Name;
}
