using Avalonia.Controls;
using Avalonia.Interactivity;

namespace FixMaster.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void Sair_Click(object? sender, RoutedEventArgs e)
    {
        Close();
    }
}
