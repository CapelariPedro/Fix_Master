namespace FixMaster.Infrastructure;

public sealed class DashboardIndicadores
{
    public int Marcados { get; set; }
    public int Total => Marcados;
    public int Pendentes { get; set; }
    public int EmAndamento { get; set; }
    public int Concluidos { get; set; }

    public static DashboardIndicadores Vazio => new()
    {
        Marcados = 0,
        Pendentes = 0,
        EmAndamento = 0,
        Concluidos = 0
    };
}
