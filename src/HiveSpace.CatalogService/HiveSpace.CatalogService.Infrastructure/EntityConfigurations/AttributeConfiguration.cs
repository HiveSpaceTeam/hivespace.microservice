using HiveSpace.CatalogService.Domain.Aggregates.AttributeAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HiveSpace.CatalogService.Infrastructure.EntityConfigurations;

public class AttributeConfiguration : IEntityTypeConfiguration<AttributeDefinition>
{
    public void Configure(EntityTypeBuilder<AttributeDefinition> entity)
    {
        entity.ToTable("attributes");
        entity.HasKey(a => a.Id);

        entity.OwnsOne(a => a.Type, at =>
        {
            at.Property(t => t.ValueType).HasColumnName("ValueType");
            at.Property(t => t.InputType).HasColumnName("InputType");
            at.Property(t => t.IsMandatory).HasColumnName("IsMandatory");
            at.Property(t => t.MaxValueCount).HasColumnName("MaxValueCount");
        });
    }
}
