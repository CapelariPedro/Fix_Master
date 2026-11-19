using FixMaster.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FixMaster.Data.Configurations;

public sealed class OrdemServicoConfiguration : IEntityTypeConfiguration<OrdemServico>
{
    public void Configure(EntityTypeBuilder<OrdemServico> builder)
    {
        builder.ToTable("OrdensServico", table =>
        {
            table.HasCheckConstraint(
                "CK_OrdensServico_MaoDeObra",
                "\"MaoDeObra\" >= 0");
            table.HasCheckConstraint(
                "CK_OrdensServico_Status",
                "\"Status\" >= 0 AND \"Status\" <= 1");
        });

        builder.HasKey(ordem => ordem.Id);
        builder.Property(ordem => ordem.RelatorioTecnico)
            .IsRequired()
            .HasMaxLength(5000);
        builder.Property(ordem => ordem.MaoDeObra)
            .IsRequired()
            .HasColumnType("TEXT")
            .HasPrecision(10, 2);
        builder.Property(ordem => ordem.Status)
            .IsRequired()
            .HasConversion<int>()
            .ValueGeneratedNever();
        builder.Property(ordem => ordem.DataEmissao).IsRequired();

        builder.HasOne(ordem => ordem.Agendamento)
            .WithOne()
            .HasForeignKey<OrdemServico>(ordem => ordem.AgendamentoId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(ordem => ordem.AgendamentoId).IsUnique();
    }
}
