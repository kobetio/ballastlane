using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyLibrary.Domain.Entities;

namespace MyLibrary.Infrastructure.Persistence.Configurations;

public class BookConfiguration : IEntityTypeConfiguration<Book>
{
    public void Configure(EntityTypeBuilder<Book> builder)
    {
        builder.ToTable("Books");

        builder.HasKey(b => b.Id);

        builder.Property(b => b.Title)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(b => b.Author)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(b => b.Genre)
            .HasMaxLength(50);

        builder.Property(b => b.Notes)
            .HasMaxLength(1000);

        builder.Property(b => b.ReadingStatus)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.HasIndex(b => b.UserId);
    }
}
