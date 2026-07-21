using HiveSpace.PaymentService.Domain.Aggregates.Payments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HiveSpace.PaymentService.Infrastructure.EntityConfigurations.Payments;

public class PaymentLinkedOrderEntityConfiguration : IEntityTypeConfiguration<PaymentLinkedOrder>
{
    public void Configure(EntityTypeBuilder<PaymentLinkedOrder> builder)
    {
        builder.HasKey(x => x.Id);
        builder.ToTable("payment_linked_orders");

        builder.Property(x => x.OrderCode).HasMaxLength(40).IsRequired();
        builder.Property(x => x.CurrencyCode).HasMaxLength(3).IsRequired();
        builder.Property(x => x.StatusSnapshot).HasMaxLength(50);

        builder.HasIndex(x => x.OrderId);
        builder.HasIndex(x => new { x.PaymentId, x.OrderId }).IsUnique();
    }
}
