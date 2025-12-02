using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using System.Linq;
using Avalonia.Markup.Xaml;
using VetoPro.ViewModels;
using VetoPro.Views;
using Microsoft.Extensions.DependencyInjection;
using VetoPro.Services;

namespace VetoPro;

public partial class App : Application
{
    /// <summary>
    /// Gets the singleton instance of the ServiceProvider.
    /// </summary>
    public static IServiceProvider Services { get; private set; } = null!;
    
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        var services = new ServiceCollection();
        
        // Use AddSingleton so the same storage instance is used everywhere
        services.AddSingleton<ISecureStorageService, SecureStorageService>();
        
        // Register the AuthTokenHandler as Transient (it's middleware for HttpClient)
        services.AddTransient<AuthTokenHandler>();
        
        // Register the AuthService using AddHttpClient.
        // This creates an HttpClient instance specifically for AuthService
        // and automatically adds our AuthTokenHandler to its request pipeline.
        services.AddHttpClient<AuthService>(client =>
            {
                // IMPORTANT: Replace this with your API's base URL
                client.BaseAddress = new Uri("https://localhost:7008"); // Example: Your local API URL
            })
            .AddHttpMessageHandler<AuthTokenHandler>();
            
        // --- Register Other Services ---
        // Register other services your app will need (e.g., PatientService)
        // services.AddHttpClient<PatientService>(...)
        
        // --- Register ViewModels ---
        // Register ViewModels as Transient (a new one for each View)
        services.AddTransient<LoginPageViewModel>();
        // services.AddTransient<MainViewModel>();
        // services.AddTransient<PatientListViewModel>();
        
        // Build the ServiceProvider
        Services = services.BuildServiceProvider();
        
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Avoid duplicate validations from both Avalonia and the CommunityToolkit. 
            // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
            DisableAvaloniaDataAnnotationValidation();
            
            desktop.MainWindow = new MainWindow {
                DataContext = Services.GetRequiredService<LoginPageViewModel>()
            };
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            singleViewPlatform.MainView = new LoginPage {
                DataContext = Services.GetRequiredService<LoginPageViewModel>()
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void DisableAvaloniaDataAnnotationValidation()
    {
        // Get an array of plugins to remove
        var dataValidationPluginsToRemove =
            BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

        // remove each entry found
        foreach (var plugin in dataValidationPluginsToRemove)
        {
            BindingPlugins.DataValidators.Remove(plugin);
        }
    }
}