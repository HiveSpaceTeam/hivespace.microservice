﻿namespace HiveSpace.Domain.Shared;

public interface IAuditable
{
    DateTimeOffset CreatedAt { get; }
    DateTimeOffset? UpdatedAt { get; }
}