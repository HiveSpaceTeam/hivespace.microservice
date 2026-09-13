namespace HiveSpace.Infrastructure.Persistence.Seeding;

public enum SeedKind
{
    ReferenceData = 0,
    BootstrapData = 1,
    SampleData = 2
}

public interface ISeeder
{
    int Order { get; }
    SeedKind Kind => SeedKind.BootstrapData;
    Task SeedAsync(CancellationToken ct = default);
}
