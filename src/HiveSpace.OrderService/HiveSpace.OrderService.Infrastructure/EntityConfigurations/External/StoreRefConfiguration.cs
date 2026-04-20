using HiveSpace.OrderService.Domain.External;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HiveSpace.OrderService.Infrastructure.EntityConfigurations.External;

public class StoreRefConfiguration : IEntityTypeConfiguration<StoreRef>
{
    public void Configure(EntityTypeBuilder<StoreRef> builder)
    {
        builder.ToTable("store_refs");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name).IsRequired().HasMaxLength(255);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(50).IsRequired();
        builder.Property(x => x.OwnerId).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.UpdatedAt);
    }
}
