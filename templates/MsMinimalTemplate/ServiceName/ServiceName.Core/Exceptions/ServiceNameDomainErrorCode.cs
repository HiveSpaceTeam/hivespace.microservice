using HiveSpace.Domain.Shared.Exceptions;

namespace ServiceName.Core.Exceptions;

public class ServiceNameDomainErrorCode : DomainErrorCode
{
    private ServiceNameDomainErrorCode(int id, string name, string code) : base(id, name, code) { }

    // TODO: Replace SVC with the service prefix (e.g. MDA for MediaService → MDA5xxx)
    // TODO: Replace N with the next available block number
    public static readonly ServiceNameDomainErrorCode EntityNotFound = new(1, "EntityNotFound", "SVC001");
}
