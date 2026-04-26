using HiveSpace.Domain.Shared.Exceptions;

namespace ServiceName.Domain.Exceptions;

public class ServiceNameDomainErrorCode : DomainErrorCode
{
    private ServiceNameDomainErrorCode(int id, string name, string code) : base(id, name, code) { }

    // TODO: Replace SVC with the service prefix (e.g. PAY for PaymentService → PAY4xxx)
    // TODO: Replace N with the next available block number
    public static readonly ServiceNameDomainErrorCode EntityNotFound = new(1, "EntityNotFound", "SVC001");
}
