namespace HiveSpace.Core.IdGeneration;

public sealed class SnowflakeOptions
{
    public const string SectionName = "Snowflake";

    /// <summary>Datacenter identifier (0–31).</summary>
    public long DatacenterId { get; set; } = 0;

    /// <summary>Machine / worker identifier (0–31).</summary>
    public long MachineId { get; set; } = 0;
}
