using HiveSpace.MediaService.Func.Core.DomainModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HiveSpace.MediaService.Func.Infrastructure.Data.EntityConfiguration;

public class MediaAssetEntityConfiguration : IEntityTypeConfiguration<MediaAsset>
{
    public void Configure(EntityTypeBuilder<MediaAsset> builder)
    {
        builder.ToTable("media_files");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.FileName)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(x => x.OriginalFileName)
            .HasMaxLength(255);

        builder.Property(x => x.StoragePath)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(x => x.PublicUrl)
            .HasMaxLength(500);

        builder.Property(x => x.MimeType)
            .HasMaxLength(100);

        builder.Property(x => x.FileSize);

        builder.Property(x => x.EntityType)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.EntityId);

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasDefaultValue(MediaStatus.Pending);

        builder.Ignore(x => x.DomainEvents);
    }
}
