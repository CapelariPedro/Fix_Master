using FixMaster.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FixMaster.Data.Configurations;

public sealed class ItemOSConfiguration : IEntityTypeConfiguration<ItemOS>
{
    public void Configure(EntityTypeBuilder<ItemOS> builder)
    {
        builder.ToTable("ItensOS", table =>
        {
            table.HasCheckConstraint(
                "CK_ItensOS_QuantidadeUtilizada",
                "\"QuantidadeUtilizada\" > 0");
            table.HasCheckConstraint(
                "CK_ItensOS_ValorUnitario",
                "\"ValorUnitario\" >= 0");
        });

        builder.HasKey(item => item.Id);
        builder.Property(item => item.QuantidadeUtilizada).IsRequired();
        builder.Property(item => item.ValorUnitario)
            .IsRequired()
            .HasColumnType("TEXT")
            .HasPrecision(10, 2);
        builder.Ignore(item => item.Subtotal);

        builder.HasOne(item => item.OrdemServico)
            .WithMany(ordem => ordem.Itens)
            .HasForeignKey(item => item.OrdemServicoId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(item => item.ItemEstoque)
            .WithMany()
            .HasForeignKey(item => item.ItemEstoqueId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(item => new { item.OrdemServicoId, item.ItemEstoqueId })
            .IsUnique();
    }
}
