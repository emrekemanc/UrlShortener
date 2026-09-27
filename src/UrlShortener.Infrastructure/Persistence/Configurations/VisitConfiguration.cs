using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UrlShortener.Domain.ShortUrls;
using UrlShortener.Domain.Visits;

namespace UrlShortener.Infrastructure.Persistence.Configurations;

internal sealed class VisitConfiguration : IEntityTypeConfiguration<Visit>
{
    public void Configure(EntityTypeBuilder<Visit> builder)
    {
        builder.Property(visit => visit.Id)
            .HasConversion(id => id.Value, value => new VisitId(value))
            .ValueGeneratedNever();

        builder.Property(visit => visit.ShortUrlId)
            .HasConversion(id => id.Value, value => new ShortUrlId(value));

        builder.HasOne<ShortUrl>()
            .WithMany()
            .HasForeignKey(visit => visit.ShortUrlId);

        builder.HasIndex(visit => new { visit.ShortUrlId, visit.VisitedAtUtc });

        builder.Ignore(visit => visit.DomainEvents);
    }
}
