using HiveSpace.IdentityService.Core.DomainModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HiveSpace.IdentityService.Core.Persistence.EntityConfigurations;

public class OtpChallengeConfiguration : IEntityTypeConfiguration<OtpChallenge>
{
    public void Configure(EntityTypeBuilder<OtpChallenge> builder)
    {
        builder.ToTable("otp_challenges");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasConversion(
                value => value.Value,
                value => new OtpChallengeId(value));

        builder.Property(x => x.EmailNormalized)
            .HasColumnName("email_normalized")
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(x => x.Purpose)
            .HasColumnName("purpose")
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.ChallengeToken)
            .HasColumnName("challenge_token")
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(x => x.Code)
            .HasColumnName("code")
            .HasMaxLength(6)
            .IsRequired();

        builder.Property(x => x.ExpiresAt).HasColumnName("expires_at").IsRequired();
        builder.Property(x => x.CanResendAt).HasColumnName("can_resend_at").IsRequired();
        builder.Property(x => x.AttemptCount).HasColumnName("attempt_count").IsRequired();
        builder.Property(x => x.IsUsed).HasColumnName("is_used").IsRequired();
        builder.Property(x => x.IsInvalidated).HasColumnName("is_invalidated").IsRequired();
        builder.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();

        builder.HasIndex(x => x.ChallengeToken).IsUnique();
        builder.HasIndex(x => new { x.EmailNormalized, x.Purpose });
    }
}
