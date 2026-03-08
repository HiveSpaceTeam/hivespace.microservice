using HiveSpace.OrderService.Domain.Aggregates.Coupons;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HiveSpace.OrderService.Infrastructure.EntityConfigurations.Coupons;

public class CouponRuleEntityConfiguration : IEntityTypeConfiguration<CouponRule>
{
    public void Configure(EntityTypeBuilder<CouponRule> builder)
    {
        builder.ToTable("coupon_rules");

        builder.HasKey(r => r.Id);

        // Foreign Key
        builder.HasOne<Coupon>()
            .WithMany(c => c.Rules)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(r => r.RuleName).HasMaxLength(100).IsRequired();
        builder.Property(r => r.RuleExpression).HasMaxLength(500).IsRequired();
        builder.Property(r => r.ErrorMessage).HasMaxLength(255).IsRequired();
    }
}
