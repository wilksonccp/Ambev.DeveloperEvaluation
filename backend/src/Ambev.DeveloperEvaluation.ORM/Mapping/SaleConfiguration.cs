using Ambev.DeveloperEvaluation.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ambev.DeveloperEvaluation.ORM.Mapping;

public class SaleConfiguration : IEntityTypeConfiguration<Sale>
{
    public void Configure(EntityTypeBuilder<Sale> builder)
    {
        builder.ToTable("Sales");

        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id)
            .HasColumnType("uuid")
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(s => s.Number)
            .IsRequired()
            .HasMaxLength(50);
        builder.HasIndex(s => s.Number).IsUnique();

        builder.Property(s => s.CustomerId).HasColumnType("uuid").IsRequired();
        builder.Property(s => s.CustomerName).IsRequired().HasMaxLength(150);

        builder.Property(s => s.BranchId).HasColumnType("uuid").IsRequired();
        builder.Property(s => s.BranchName).IsRequired().HasMaxLength(150);

        builder.Property(s => s.CreatedAt).IsRequired();
        builder.Property(s => s.UpdatedAt).IsRequired();
        builder.Property(s => s.DeletedAt);

        builder.Property(s => s.IsCancelled).IsRequired();

        builder.Property(s => s.TotalAmount).HasPrecision(18, 2);
        builder.Property(s => s.TotalDiscount).HasPrecision(18, 2);
        builder.Property(s => s.TotalPayable).HasPrecision(18, 2);

        builder.HasIndex(s => s.CustomerId);
        builder.HasIndex(s => s.BranchId);
        builder.HasIndex(s => s.CreatedAt);

        // Ignore read-only projection to avoid EF trying to map it as a relationship
        builder.Ignore(s => s.ReadOnlyItems);

        builder.OwnsMany<SaleItem>("Items", b =>
        {
            b.ToTable("SaleItems");
            b.WithOwner().HasForeignKey("SaleId");

            b.Property<Guid>("Id")
                .HasColumnType("uuid")
                .HasDefaultValueSql("gen_random_uuid()");
            b.HasKey("Id");

            b.Property(si => si.ProductId).HasColumnType("uuid").IsRequired();
            b.Property(si => si.ProductName).IsRequired().HasMaxLength(200);
            b.Property(si => si.Quantity).IsRequired();
            b.Property(si => si.UnitPrice).HasPrecision(18, 2).IsRequired();
            b.Property(si => si.DiscountAmount).HasPrecision(18, 2).IsRequired();
            b.Property(si => si.LineTotal).HasPrecision(18, 2).IsRequired();
            b.Property(si => si.IsCancelled).IsRequired();

            b.HasIndex("SaleId");
            b.HasIndex(si => si.ProductId);
        });

        builder.Navigation("Items").UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation("Items").AutoInclude();
    }
}
