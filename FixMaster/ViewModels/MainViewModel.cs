using FixMaster.Services.Interfaces;

namespace FixMaster.ViewModels;

public sealed class MainViewModel : ViewModelBase
{
    private object _currentScreen;

    public MainViewModel(IAuthService auth)
    {
        _currentScreen = new LoginViewModel(
            auth,
            () => CurrentScreen = new ShellViewModel());
    }

    public object CurrentScreen
    {
        get => _currentScreen;
        private set => SetProperty(ref _currentScreen, value);
    }
}
