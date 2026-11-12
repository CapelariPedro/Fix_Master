using FixMaster.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FixMaster.Data.Configurations;

public sealed class AgendamentoConfiguration : IEntityTypeConfiguration<Agendamento>
{
    public void Configure(EntityTypeBuilder<Agendamento> builder)
    {
        builder.ToTable("Agendamentos", table =>
        {
            table.HasCheckConstraint(
                "CK_Agendamentos_DuracaoMinutos",
                "\"DuracaoMinutos\" > 0 AND \"DuracaoMinutos\" <= 1440");
            table.HasCheckConstraint(
                "CK_Agendamentos_Status",
                "\"Status\" >= 0 AND \"Status\" <= 3");
        });

        builder.HasKey(agendamento => agendamento.Id);
        builder.Property(agendamento => agendamento.DataHoraInicio).IsRequired();
        builder.Property(agendamento => agendamento.DuracaoMinutos).IsRequired();
        builder.Property(agendamento => agendamento.DataHoraFim).IsRequired();
        builder.Property(agendamento => agendamento.DescricaoServico)
            .IsRequired()
            .HasMaxLength(500);
        builder.Property(agendamento => agendamento.Status)
            .IsRequired()
            .HasConversion<int>()
            .HasDefaultValue(StatusAgendamento.Pendente);

        builder.HasOne(agendamento => agendamento.Cliente)
            .WithMany()
            .HasForeignKey(agendamento => agendamento.ClienteId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(agendamento => agendamento.DataHoraInicio);
        builder.HasIndex(agendamento => agendamento.ClienteId);
    }
}