using System;
using System.Globalization;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;

namespace FixMaster.Views;

public partial class MainWindow : Window
{
    private readonly DispatcherTimer _relogio;

    public MainWindow()
    {
        InitializeComponent();

        AtualizarRelogio();
        _relogio = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _relogio.Tick += (_, _) => AtualizarRelogio();
        _relogio.Start();
    }

    private void AtualizarRelogio()
    {
        RelogioRodape.Text = DateTime.Now.ToString("dd/MM - HH:mm:ss", CultureInfo.InvariantCulture);
    }

    protected override void OnClosed(EventArgs e)
    {
        _relogio.Stop();
        base.OnClosed(e);
    }

    private void Sair_Click(object? sender, RoutedEventArgs e)
    {
        Close();
    }
}