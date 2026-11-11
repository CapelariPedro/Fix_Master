using FixMaster.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FixMaster.Data.Configurations;

public sealed class ClienteConfiguration : IEntityTypeConfiguration<Cliente>
{
    public void Configure(EntityTypeBuilder<Cliente> builder)
    {
        builder.ToTable("Clientes");
        builder.HasKey(cliente => cliente.Id);

        builder.Property(cliente => cliente.Nome)
            .IsRequired()
            .HasMaxLength(150)
            .UseCollation("NOCASE");

        builder.Property(cliente => cliente.Documento)
            .IsRequired()
            .HasMaxLength(14);

        builder.HasIndex(cliente => cliente.Documento).IsUnique();

        builder.Property(cliente => cliente.Cep)
            .IsRequired()
            .HasMaxLength(8)
            .HasDefaultValue("");

        builder.Property(cliente => cliente.Telefone)
            .IsRequired()
            .HasMaxLength(30);

        builder.Property(cliente => cliente.Endereco)
            .IsRequired()
            .HasMaxLength(250);

        builder.Property(cliente => cliente.EhEmpresa).IsRequired();
        builder.Property(cliente => cliente.Ativo)
            .IsRequired()
            .HasDefaultValue(true);
    }
}
