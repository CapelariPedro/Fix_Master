using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using FixMaster.Data;
using FixMaster.Services;
using FixMaster.Services.Interfaces;
using FixMaster.ViewModels;
using FixMaster.Views;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FixMaster;

public partial class App : Application
{
    private ServiceProvider? _services;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var services = new ServiceCollection();
            services.AddDbContextFactory<AppDbContext>(options =>
                options.UseSqlite($"Data Source={DbPaths.DatabaseFile}"));
            services.AddSingleton<IAuthService, AuthService>();
            services.AddSingleton<MainViewModel>();
            _services = services.BuildServiceProvider();

            var factory = _services.GetRequiredService<IDbContextFactory<AppDbContext>>();
            using (var db = factory.CreateDbContext())
                db.Database.Migrate();

            desktop.MainWindow = new MainWindow
            {
                DataContext = _services.GetRequiredService<MainViewModel>()
            };
            desktop.Exit += (_, _) => _services.Dispose();
        }

        base.OnFrameworkInitializationCompleted();
    }
}
