namespace HiveSpace.Core.Contexts;

public interface IRequestContext
{
    string? RequestId { get; set; }
}