using HiveSpace.PaymentService.Domain.Aggregates.Wallets;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HiveSpace.PaymentService.Infrastructure.EntityConfigurations.Wallets;

public class WalletEntityConfiguration : IEntityTypeConfiguration<Wallet>
{
    public void Configure(EntityTypeBuilder<Wallet> builder)
    {
        builder.HasKey(w => w.Id);
        builder.ToTable("wallets");

        builder.Property(w => w.UserId).IsRequired();
        builder.Property(w => w.RewardPoints).IsRequired();
        builder.Property(w => w.SuspensionReason).HasMaxLength(500);

        builder.Property(w => w.Status)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.OwnsOne(w => w.AvailableBalance, money =>
        {
            money.Property(m => m.Amount).HasColumnName("AvailableBalance_Amount").IsRequired();
            money.Property(m => m.Currency).HasColumnName("AvailableBalance_Currency")
                .HasConversion<string>().HasMaxLength(3).IsRequired();
        });

        builder.OwnsOne(w => w.EscrowBalance, money =>
        {
            money.Property(m => m.Amount).HasColumnName("EscrowBalance_Amount").IsRequired();
            money.Property(m => m.Currency).HasColumnName("EscrowBalance_Currency")
                .HasConversion<string>().HasMaxLength(3).IsRequired();
        });

        builder.Ignore(w => w.TotalBalance);

        builder.HasMany(w => w.Transactions)
            .WithOne()
            .HasForeignKey(t => t.WalletId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(w => w.UserId).IsUnique();
    }
}
