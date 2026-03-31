using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HiveSpace.OrderService.Infrastructure.Sagas;

public class FulfillmentSagaStateEntityConfiguration : IEntityTypeConfiguration<FulfillmentSagaState>
{
    private static readonly JsonSerializerOptions _jsonOptions = new();

    public void Configure(EntityTypeBuilder<FulfillmentSagaState> builder)
    {
        builder.HasKey(s => s.CorrelationId);
        builder.ToTable("fulfillment_saga_states");

        builder.Property(s => s.CurrentState).HasMaxLength(64).IsRequired();
        builder.Property(s => s.FailureReason).HasMaxLength(500);

        builder.Property(s => s.ReservationIds)
            .HasConversion(
                v => JsonSerializer.Serialize(v, _jsonOptions),
                v => JsonSerializer.Deserialize<List<Guid>>(v, _jsonOptions) ?? new())
            .HasColumnType("nvarchar(max)");
    }
}
