using HiveSpace.PaymentService.Domain.Aggregates.Wallets;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HiveSpace.PaymentService.Infrastructure.EntityConfigurations.Wallets;

public class TransactionEntityConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.HasKey(t => t.Id);
        builder.ToTable("wallet_transactions");

        builder.Property(t => t.WalletId).IsRequired();
        builder.Property(t => t.Reference).IsRequired().HasMaxLength(200);
        builder.Property(t => t.Description).IsRequired().HasMaxLength(500);

        builder.Property(t => t.Type)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(t => t.Direction)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.OwnsOne(t => t.Amount, money =>
        {
            money.Property(m => m.Amount).HasColumnName("Amount").IsRequired();
            money.Property(m => m.Currency).HasColumnName("Currency")
                .HasConversion<string>().HasMaxLength(3).IsRequired();
        });

        builder.OwnsOne(t => t.BalanceAfter, money =>
        {
            money.Property(m => m.Amount).HasColumnName("BalanceAfter_Amount").IsRequired();
            money.Property(m => m.Currency).HasColumnName("BalanceAfter_Currency")
                .HasConversion<string>().HasMaxLength(3).IsRequired();
        });

        builder.HasIndex(t => new { t.WalletId, t.TransactedAt });
    }
}
