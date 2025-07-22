using System;

namespace HiveSpace.Domain.Shared.Errors;

public interface IErrorCode
{
    string Code { get; }
    string Name { get; }
}
