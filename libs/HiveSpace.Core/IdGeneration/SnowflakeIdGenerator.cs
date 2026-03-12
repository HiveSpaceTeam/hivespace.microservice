using HiveSpace.Core.Exceptions;
using HiveSpace.Core.Exceptions.Models;
using HiveSpace.Domain.Shared.IdGeneration;
using Microsoft.Extensions.Options;

namespace HiveSpace.Core.IdGeneration;

/// <summary>
/// Generates globally unique 64-bit IDs using the Twitter Snowflake algorithm.
/// Layout (64 bits):
///   1 bit  – always 0 (sign)
///  41 bits – milliseconds since <see cref="Epoch"/>
///   5 bits – datacenter ID  (0–31)
///   5 bits – machine ID     (0–31)
///  12 bits – per-millisecond sequence (0–4095)
/// </summary>
public sealed class SnowflakeIdGenerator : IIdGenerator<long>
{
    // 2021-01-01 00:00:00 UTC
    private const long Epoch = 1_609_459_200_000L;

    private const int DatacenterIdBits = 5;
    private const int MachineIdBits    = 5;
    private const int SequenceBits     = 12;

    private const long MaxDatacenterId = (1L << DatacenterIdBits) - 1; // 31
    private const long MaxMachineId    = (1L << MachineIdBits)    - 1; // 31
    private const long MaxSequence     = (1L << SequenceBits)     - 1; // 4095

    // Bit-shift offsets
    private const int MachineShift    = SequenceBits;                                  // 12
    private const int DatacenterShift = SequenceBits + MachineIdBits;                  // 17
    private const int TimestampShift  = SequenceBits + MachineIdBits + DatacenterIdBits; // 22

    private readonly long _datacenterId;
    private readonly long _machineId;

    private long _sequence     = 0L;
    private long _lastTimestamp = -1L;
    private readonly object _lock = new();

    public SnowflakeIdGenerator(IOptions<SnowflakeOptions> options)
    {
        var cfg = options.Value;

        if (cfg.DatacenterId is < 0 or > MaxDatacenterId)
        {
            var error = new Error(CommonErrorCode.ConfigurationMissing, nameof(cfg.DatacenterId));
            throw new ConfigurationException([error]);
        }

        if (cfg.MachineId is < 0 or > MaxMachineId)
        {
            var error = new Error(CommonErrorCode.ConfigurationMissing, nameof(cfg.MachineId));
            throw new ConfigurationException([error]);
        }

        _datacenterId = cfg.DatacenterId;
        _machineId    = cfg.MachineId;
    }

    public long NewId()
    {
        lock (_lock)
        {
            var timestamp = CurrentTimestamp();

            if (timestamp < _lastTimestamp)
                throw new InvalidOperationException(
                    $"Clock moved backwards by {_lastTimestamp - timestamp} ms. Refusing to generate ID.");

            if (timestamp == _lastTimestamp)
            {
                _sequence = (_sequence + 1) & MaxSequence;

                if (_sequence == 0)
                    timestamp = WaitNextMillis(_lastTimestamp);
            }
            else
            {
                _sequence = 0;
            }

            _lastTimestamp = timestamp;

            return ((timestamp - Epoch) << TimestampShift)
                 | (_datacenterId       << DatacenterShift)
                 | (_machineId          << MachineShift)
                 | _sequence;
        }
    }

    private static long CurrentTimestamp() =>
        DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

    private static long WaitNextMillis(long lastTimestamp)
    {
        var ts = CurrentTimestamp();
        while (ts <= lastTimestamp) ts = CurrentTimestamp();
        return ts;
    }
}
