namespace FixMaster.Infrastructure;

public sealed record UsuarioResumo(int Id, string Username, bool EhAdministrador)
{
    public string Perfil => EhAdministrador ? "ADMINISTRADOR" : "OPERADOR";
}
