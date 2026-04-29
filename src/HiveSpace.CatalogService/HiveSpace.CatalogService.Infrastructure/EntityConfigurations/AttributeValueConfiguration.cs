using HiveSpace.CatalogService.Domain.Aggregates.AttributeAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HiveSpace.CatalogService.Infrastructure.EntityConfigurations;

public class AttributeValueConfiguration : IEntityTypeConfiguration<AttributeValue>
{
    public void Configure(EntityTypeBuilder<AttributeValue> entity)
    {
        entity.ToTable("attribute_values");
        entity.HasKey(av => av.Id);
    }
}
