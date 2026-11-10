namespace FixMaster.Models;

public class Usuario
{
    public int Id { get; set; }
    public string Username { get; set; } = "";
    public bool EhAdministrador { get; set; }

    // Formato: PBKDF2-SHA256$iterações$salt(base64)$hash(base64). Nunca a senha em texto.
    public string SenhaHash { get; set; } = "";
}
