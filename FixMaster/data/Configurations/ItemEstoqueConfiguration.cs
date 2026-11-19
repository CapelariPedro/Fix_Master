using FixMaster.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FixMaster.Data.Configurations;

public sealed class ItemEstoqueConfiguration : IEntityTypeConfiguration<ItemEstoque>
{
    public void Configure(EntityTypeBuilder<ItemEstoque> builder)
    {
        builder.ToTable("ItensEstoque", table =>
        {
            table.HasCheckConstraint(
                "CK_ItensEstoque_Quantidade",
                "\"Quantidade\" >= 0");
            table.HasCheckConstraint(
                "CK_ItensEstoque_Prateleira",
                "length(\"Prateleira\") = 1 AND \"Prateleira\" >= 'A' AND \"Prateleira\" <= 'G'");
            table.HasCheckConstraint(
                "CK_ItensEstoque_Posicao",
                "\"Posicao\" >= 1 AND \"Posicao\" <= 8");
        });

        builder.HasKey(item => item.Id);
        builder.Property(item => item.Codigo)
            .IsRequired()
            .HasMaxLength(40)
            .UseCollation("NOCASE");
        builder.HasIndex(item => item.Codigo).IsUnique();

        builder.Property(item => item.Nome)
            .IsRequired()
            .HasMaxLength(150)
            .UseCollation("NOCASE");
        builder.Property(item => item.Descricao)
            .IsRequired()
            .HasMaxLength(500)
            .HasDefaultValue("");
        builder.Property(item => item.Categoria)
            .IsRequired()
            .HasMaxLength(80)
            .UseCollation("NOCASE");
        builder.Property(item => item.Quantidade).IsRequired();
        builder.Property(item => item.PrecoUnitario)
            .IsRequired()
            .HasColumnType("TEXT");
        builder.Property(item => item.Prateleira)
            .IsRequired()
            .HasMaxLength(1);
        builder.Property(item => item.Posicao).IsRequired();
        builder.Property(item => item.Ativo)
            .IsRequired()
            .HasDefaultValue(true);

        builder.HasIndex(item => new { item.Prateleira, item.Posicao })
            .IsUnique()
            .HasFilter("\"Ativo\" = 1");
    }
}
