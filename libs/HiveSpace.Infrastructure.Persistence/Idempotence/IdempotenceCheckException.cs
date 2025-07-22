using System;

namespace HiveSpace.Infrastructure.Persistence.Idempotence;

public class IdempotenceCheckException : Exception
{
    private readonly string _requestId;

    public IdempotenceCheckException(string requestId)
    {
        _requestId = requestId;
    }
}
