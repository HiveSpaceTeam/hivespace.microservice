namespace HiveSpace.Domain.Shared;
public class DomainErrorCode(int id, string name, string code) : Enumeration(id, name)
{
    public string Code { get; private set; } = code;
}
