using HiveSpace.PaymentService.Domain.Aggregates.Payments;
using HiveSpace.PaymentService.Domain.Aggregates.Payments.Enumerations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HiveSpace.PaymentService.Infrastructure.EntityConfigurations.Payments;

public class PaymentEntityConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.HasKey(p => p.Id);
        builder.ToTable("payments");

        builder.Property(p => p.OrderId).IsRequired();
        builder.Property(p => p.BuyerId).IsRequired();
        builder.Property(p => p.IdempotencyKey).IsRequired().HasMaxLength(200);
        builder.Property(p => p.GatewayTransactionId).HasMaxLength(200);
        builder.Property(p => p.GatewayPaymentUrl).HasMaxLength(2000);

        builder.Property(p => p.Status)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(p => p.Gateway)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.OwnsOne(p => p.Amount, money =>
        {
            money.Property(m => m.Amount).HasColumnName("Amount").IsRequired();
            money.Property(m => m.Currency).HasColumnName("Currency")
                .HasConversion<string>().HasMaxLength(3).IsRequired();
        });

        builder.OwnsOne(p => p.PaymentMethod, pm =>
        {
            pm.Property(m => m.Type).HasColumnName("PaymentMethod_Type")
                .HasConversion<string>().HasMaxLength(50).IsRequired();
            pm.Property(m => m.CardLast4).HasColumnName("PaymentMethod_CardLast4").HasMaxLength(4);
            pm.Property(m => m.CardBrand).HasColumnName("PaymentMethod_CardBrand").HasMaxLength(50);
            pm.Property(m => m.WalletProvider).HasColumnName("PaymentMethod_WalletProvider").HasMaxLength(50);
            pm.Property(m => m.BankCode).HasColumnName("PaymentMethod_BankCode").HasMaxLength(20);
        });

        builder.OwnsOne(p => p.GatewayResponse, gr =>
        {
            gr.Property(r => r.RawResponse).HasColumnName("GatewayResponse_RawResponse");
            gr.Property(r => r.Success).HasColumnName("GatewayResponse_Success");
            gr.Property(r => r.ErrorMessage).HasColumnName("GatewayResponse_ErrorMessage").HasMaxLength(500);
        });

        builder.HasIndex(p => p.IdempotencyKey).IsUnique();
        builder.HasIndex(p => p.OrderId).IsUnique();
    }
}
