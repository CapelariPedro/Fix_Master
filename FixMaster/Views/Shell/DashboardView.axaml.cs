using System;
using System.Globalization;
using Avalonia.Controls;
using Avalonia.Threading;

namespace FixMaster.Views.Shell;

public partial class DashboardView : UserControl
{
    private readonly DispatcherTimer _relogio;

    public DashboardView()
    {
        InitializeComponent();

        _relogio = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _relogio.Tick += (_, _) => AtualizarRelogio();
        AtualizarRelogio();
    }

    protected override void OnAttachedToVisualTree(Avalonia.VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        AtualizarRelogio();
        _relogio.Start();
    }

    protected override void OnDetachedFromVisualTree(Avalonia.VisualTreeAttachmentEventArgs e)
    {
        _relogio.Stop();
        base.OnDetachedFromVisualTree(e);
    }

    private void AtualizarRelogio()
    {
        var agora = DateTime.Now;
        DataTitulo.Text = agora.ToString("dd/MM", CultureInfo.InvariantCulture);
    }
}