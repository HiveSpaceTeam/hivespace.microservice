namespace HiveSpace.Infrastructure.Persistence.Seeding;

public interface ISeeder
{
    int Order { get; }
    Task SeedAsync(CancellationToken ct = default);
}
