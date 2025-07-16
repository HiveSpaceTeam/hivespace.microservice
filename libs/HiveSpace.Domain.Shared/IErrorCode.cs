using System;

namespace HiveSpace.Domain.Shared;

public interface IErrorCode
{
    string Code { get; }
    string Name { get; }
}
