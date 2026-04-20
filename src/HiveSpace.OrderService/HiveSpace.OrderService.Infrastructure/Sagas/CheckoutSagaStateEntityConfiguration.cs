using System.Text.Json;
using HiveSpace.Domain.Shared.Entities;
using HiveSpace.Domain.Shared.Enumerations;
using HiveSpace.Infrastructure.Messaging.Shared.CheckoutSaga.Dtos;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HiveSpace.OrderService.Infrastructure.Sagas;

public class CheckoutSagaStateEntityConfiguration : IEntityTypeConfiguration<CheckoutSagaState>
{
    private static readonly JsonSerializerOptions _jsonOptions = new();

    public void Configure(EntityTypeBuilder<CheckoutSagaState> builder)
    {
        builder.HasKey(s => s.CorrelationId);
        builder.ToTable("checkout_saga_states");

        builder.Property(s => s.CurrentState).HasMaxLength(64).IsRequired();
        builder.Property(s => s.FailureReason).HasMaxLength(500);

        builder.Property(s => s.PaymentMethod)
            .HasConversion(
                v => v.Name,
                v => Enumeration.FromDisplayName<PaymentMethod>(v))
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(s => s.CouponCodes)
            .HasConversion(
                v => JsonSerializer.Serialize(v, _jsonOptions),
                v => JsonSerializer.Deserialize<List<string>>(v, _jsonOptions) ?? new())
            .HasColumnType("nvarchar(max)");

        builder.Property(s => s.OrderIds)
            .HasConversion(
                v => JsonSerializer.Serialize(v, _jsonOptions),
                v => JsonSerializer.Deserialize<List<Guid>>(v, _jsonOptions) ?? new())
            .HasColumnType("nvarchar(max)");

        builder.Property(s => s.ReservationIds)
            .HasConversion(
                v => JsonSerializer.Serialize(v, _jsonOptions),
                v => JsonSerializer.Deserialize<List<Guid>>(v, _jsonOptions) ?? new())
            .HasColumnType("nvarchar(max)");

        builder.Property(s => s.DeliveryAddress)
            .HasConversion(
                v => JsonSerializer.Serialize(v, _jsonOptions),
                v => JsonSerializer.Deserialize<DeliveryAddressDto>(v, _jsonOptions)!)
            .HasColumnType("nvarchar(1000)");

        builder.Property(s => s.OrderStoreMap)
            .HasConversion(
                v => JsonSerializer.Serialize(v, _jsonOptions),
                v => JsonSerializer.Deserialize<Dictionary<Guid, Guid>>(v, _jsonOptions) ?? new())
            .HasColumnType("nvarchar(max)");

        builder.Property(s => s.OrderCodeMap)
            .HasConversion(
                v => JsonSerializer.Serialize(v, _jsonOptions),
                v => JsonSerializer.Deserialize<Dictionary<Guid, string>>(v, _jsonOptions) ?? new())
            .HasColumnType("nvarchar(max)");

        builder.Property(s => s.OrderReservationMap)
            .HasConversion(
                v => JsonSerializer.Serialize(v, _jsonOptions),
                v => JsonSerializer.Deserialize<Dictionary<Guid, List<Guid>>>(v, _jsonOptions) ?? new())
            .HasColumnType("nvarchar(max)");
    }
}
