namespace HiveSpace.Domain.Shared.Interfaces;
public interface ISoftDeletable
{
    bool IsDeleted { get; set; }
    DateTimeOffset? DeletedAt { get; set; }
}
