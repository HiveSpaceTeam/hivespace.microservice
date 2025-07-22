namespace HiveSpace.Core.Exceptions.Models;
public record ErrorCodeDto(
    string Code,
    string MessageCode,
    string? Source
);
