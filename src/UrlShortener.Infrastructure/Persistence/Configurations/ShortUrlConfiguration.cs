using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UrlShortener.Domain.ShortUrls;

namespace UrlShortener.Infrastructure.Persistence.Configurations;

internal sealed class ShortUrlConfiguration : IEntityTypeConfiguration<ShortUrl>
{
    public void Configure(EntityTypeBuilder<ShortUrl> builder)
    {
        builder.ToTable("ShortUrls");

        builder.HasKey(shortUrl => shortUrl.Id);

        builder.Property(shortUrl => shortUrl.Id)
            .HasConversion(id => id.Value, value => new ShortUrlId(value))
            .ValueGeneratedNever();

        builder.Property(shortUrl => shortUrl.Code)
            .HasConversion(code => code.Value, value => ShortCode.Create(value).Value)
            .HasMaxLength(ShortCode.MaxLength)
            .IsRequired();

        builder.HasIndex(shortUrl => shortUrl.Code)
            .IsUnique();

        builder.Property(shortUrl => shortUrl.OriginalUrl)
            .HasConversion(url => url.Value, value => OriginalUrl.Create(value).Value)
            .HasMaxLength(OriginalUrl.MaxLength)
            .IsRequired();

        builder.Property(shortUrl => shortUrl.CreatedAtUtc).IsRequired();
        builder.Property(shortUrl => shortUrl.ExpiresAtUtc);
        builder.Property(shortUrl => shortUrl.DeactivatedAtUtc);
        builder.Property(shortUrl => shortUrl.VisitCount).IsRequired();
        builder.Property(shortUrl => shortUrl.LastVisitedAtUtc);

        builder.Ignore(shortUrl => shortUrl.DomainEvents);
    }
}
