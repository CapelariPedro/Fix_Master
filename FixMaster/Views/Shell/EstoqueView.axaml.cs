using Avalonia.Controls;
using Avalonia.Input;
using FixMaster.ViewModels;

namespace FixMaster.Views.Shell;

public partial class EstoqueView : UserControl
{
    public EstoqueView()
    {
        InitializeComponent();
    }

    private async void Busca_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter && DataContext is EstoqueViewModel viewModel)
            await viewModel.AtualizarCommand.ExecuteAsync(null);
    }

    private async void TipoMaterial_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (sender is ComboBox { SelectedItem: string categoria }
            && DataContext is EstoqueViewModel viewModel)
        {
            await viewModel.AtualizarCodigoPorCategoriaAsync(categoria);
        }
    }

    private async void TipoInsumo_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (sender is ComboBox { SelectedItem: string tipoInsumo }
            && DataContext is EstoqueViewModel viewModel)
        {
            await viewModel.AtualizarCodigoPorCategoriaAsync(
                viewModel.Categoria, tipoInsumo);
        }
    }
}
