using System.Text.Json;
using HiveSpace.Domain.Shared.Entities;
using HiveSpace.Domain.Shared.Enumerations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HiveSpace.OrderService.Infrastructure.Sagas;

public class FulfillmentSagaStateEntityConfiguration : IEntityTypeConfiguration<FulfillmentSagaState>
{
    private static readonly JsonSerializerOptions _jsonOptions = new();

    private static ValueComparer<T> JsonComparer<T>() => new(
        (c1, c2) => JsonSerializer.Serialize(c1, _jsonOptions) == JsonSerializer.Serialize(c2, _jsonOptions),
        c => JsonSerializer.Serialize(c, _jsonOptions).GetHashCode(),
        c => JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(c, _jsonOptions), _jsonOptions)!);

    public void Configure(EntityTypeBuilder<FulfillmentSagaState> builder)
    {
        builder.HasKey(s => s.CorrelationId);
        builder.ToTable("fulfillment_saga_states");

        builder.Property(s => s.CurrentState).HasMaxLength(64).IsRequired();
        builder.Property(s => s.OrderCode).HasMaxLength(64).IsRequired();
        builder.Property(s => s.FailureReason).HasMaxLength(500);

        builder.Property(s => s.PaymentMethod)
            .HasConversion(
                v => v.Name,
                v => Enumeration.FromDisplayName<PaymentMethod>(v))
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(s => s.ReservationIds)
            .HasConversion(
                v => JsonSerializer.Serialize(v, _jsonOptions),
                v => JsonSerializer.Deserialize<List<Guid>>(v, _jsonOptions) ?? new(),
                JsonComparer<List<Guid>>())
            .HasColumnType("nvarchar(max)");
    }
}
