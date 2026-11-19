using FixMaster.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FixMaster.Data.Configurations;

public class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.ToTable("Usuarios");
        builder.HasKey(u => u.Id);

        // NOCASE: "Admin" e "admin" são o mesmo usuário.
        builder.Property(u => u.Username)
            .IsRequired()
            .HasMaxLength(50)
            .UseCollation("NOCASE");

        builder.HasIndex(u => u.Username).IsUnique();

        builder.Property(u => u.SenhaHash).IsRequired();
        builder.Property(u => u.EhAdministrador).IsRequired();
    }
}
