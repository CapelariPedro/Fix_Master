using FixMaster.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FixMaster.Data.Configurations;

public sealed class AtividadeRecenteConfiguration : IEntityTypeConfiguration<AtividadeRecente>
{
    public void Configure(EntityTypeBuilder<AtividadeRecente> builder)
    {
        builder.ToTable("AtividadesRecentes");
        builder.HasKey(atividade => atividade.Id);
        builder.Property(atividade => atividade.DataHora).IsRequired();
        builder.Property(atividade => atividade.Tipo)
            .IsRequired()
            .HasMaxLength(60);
        builder.Property(atividade => atividade.Descricao)
            .IsRequired()
            .HasMaxLength(500);
        builder.HasIndex(atividade => atividade.DataHora);
        builder.HasIndex(atividade => new { atividade.Tipo, atividade.ReferenciaId });
    }
}
