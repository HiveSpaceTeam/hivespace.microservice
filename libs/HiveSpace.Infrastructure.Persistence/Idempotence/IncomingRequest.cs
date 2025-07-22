namespace HiveSpace.Infrastructure.Persistence.Idempotence;

public class IncomingRequest
{
    public string CorrelationId { get; protected set; }
    public Guid RequestId { get; protected set; }
    public string ActionName { get; set; } = string.Empty;
    public DateTimeOffset DateTimeCreated { get; protected set; }

    public IncomingRequest(string correlationId, Guid requestId, string actionName)
    {
        CorrelationId = correlationId ?? throw new ArgumentNullException(nameof(correlationId));
        RequestId = Guid.Empty.Equals(requestId) ? throw new ArgumentNullException(nameof(requestId)) : requestId;
        DateTimeCreated = DateTimeOffset.UtcNow;
        ActionName = actionName;
    }
}
