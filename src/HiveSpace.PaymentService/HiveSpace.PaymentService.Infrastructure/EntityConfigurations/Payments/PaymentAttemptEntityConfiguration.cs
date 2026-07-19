using HiveSpace.PaymentService.Domain.Aggregates.Payments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HiveSpace.PaymentService.Infrastructure.EntityConfigurations.Payments;

public class PaymentAttemptEntityConfiguration : IEntityTypeConfiguration<PaymentAttempt>
{
    public void Configure(EntityTypeBuilder<PaymentAttempt> builder)
    {
        builder.HasKey(x => x.Id);
        builder.ToTable("payment_attempts");

        builder.Property(x => x.MethodCode).HasMaxLength(50).IsRequired();
        builder.Property(x => x.GatewayCode).HasMaxLength(50);
        builder.Property(x => x.CurrencyCode).HasMaxLength(3).IsRequired();
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(50).IsRequired();
        builder.Property(x => x.GatewayTransactionId).HasMaxLength(200);
        builder.Property(x => x.RedirectUrl).HasMaxLength(2000);
        builder.Property(x => x.FailureReasonCode).HasMaxLength(100);
        builder.Property(x => x.FailureReason).HasMaxLength(500);
        builder.Property(x => x.IdempotencyKey).HasMaxLength(200).IsRequired();

        builder.HasIndex(x => new { x.PaymentId, x.AttemptNo }).IsUnique();
        builder.HasIndex(x => x.GatewayTransactionId);
    }
}
