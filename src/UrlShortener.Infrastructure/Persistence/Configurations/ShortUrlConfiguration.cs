using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UrlShortener.Domain.ShortUrls;

namespace UrlShortener.Infrastructure.Persistence.Configurations;

internal sealed class ShortUrlConfiguration : IEntityTypeConfiguration<ShortUrl>
{
    public void Configure(EntityTypeBuilder<ShortUrl> builder)
    {
        builder.Property(shortUrl => shortUrl.Id)
            .HasConversion(id => id.Value, value => new ShortUrlId(value))
            .ValueGeneratedNever();

        builder.Property(shortUrl => shortUrl.Code)
            .HasConversion(code => code.Value, value => ShortCode.Create(value).Value)
            .HasMaxLength(ShortCode.MaxLength);

        builder.HasIndex(shortUrl => shortUrl.Code).IsUnique();

        builder.Property(shortUrl => shortUrl.OriginalUrl)
            .HasConversion(url => url.Value, value => OriginalUrl.Create(value).Value)
            .HasMaxLength(OriginalUrl.MaxLength);

        builder.Ignore(shortUrl => shortUrl.DomainEvents);
    }
}
