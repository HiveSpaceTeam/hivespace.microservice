using HiveSpace.Domain.Shared.Interfaces;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.Domain.Shared.Errors;

namespace HiveSpace.Infrastructure.Persistence.Idempotence;

public class IncomingRequest
{
    public string CorrelationId { get; protected set; }
    public Guid RequestId { get; protected set; }
    public string ActionName { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; }

    public IncomingRequest(string correlationId, Guid requestId, string actionName)
    {
        CorrelationId = correlationId ?? throw new InvalidFieldException(DomainErrorCode.ArgumentNull, nameof(correlationId));
        RequestId = Guid.Empty.Equals(requestId) ? throw new InvalidFieldException(DomainErrorCode.ArgumentNull, nameof(requestId)) : requestId;
        CreatedAt = DateTimeOffset.Now;
        ActionName = actionName;
    }
}
