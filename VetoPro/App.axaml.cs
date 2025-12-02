using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using System.Linq;
using System.Net.Http;
using Avalonia.Controls;
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
        
        string baseAddress = "https://localhost:7008";
        
        if (OperatingSystem.IsAndroid())
        {
            baseAddress = "https://10.0.2.2:7008";
        }
        
        // Use AddSingleton so the same storage instance is used everywhere
        services.AddSingleton<ISecureStorageService, SecureStorageService>();
        services.AddSingleton<INavigationService, NavigationService>();
        
        // Register the AuthTokenHandler as Transient (it's middleware for HttpClient)
        services.AddTransient<AuthTokenHandler>();
        
        // Configure HttpClent
        var httpClientBuilder = services.AddHttpClient<AuthService>(client =>
        {
            client.BaseAddress = new Uri(baseAddress);
        });
        
        // Only configure the SSL bypass if we are NOT in a browser.
        // Browsers throw an exception if you try to touch ServerCertificateCustomValidationCallback.
        if (!OperatingSystem.IsBrowser())
        {
            httpClientBuilder.ConfigurePrimaryHttpMessageHandler(() => 
            {
                var handler = new HttpClientHandler();
            
                #if DEBUG
                // Allow self-signed certs on Desktop, Android, and iOS during dev
                handler.ServerCertificateCustomValidationCallback = 
                    (message, cert, chain, errors) => true;
                #endif
            
                return handler;
            });
        } 
        
        // Add the token handler to the pipeline (applies to all platforms)
        httpClientBuilder.AddHttpMessageHandler<AuthTokenHandler>();
            
        // --- Register Other Services ---
        // Register other services your app will need (e.g., PatientService)
        // services.AddHttpClient<PatientService>(...)
        
        // --- Register ViewModels ---
        // Register ViewModels as Transient (a new one for each View)
        services.AddTransient<MainViewModel>();
        services.AddTransient<LoginPageViewModel>();
        services.AddTransient<DashboardViewModel>();
        // services.AddTransient<PatientListViewModel>();
        
        // Build the ServiceProvider
        Services = services.BuildServiceProvider();
        
        // --- Démarrage avec MainViewModel (Le Shell) ---
        var mainViewModel = Services.GetRequiredService<MainViewModel>();
        
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Avoid duplicate validations from both Avalonia and the CommunityToolkit. 
            // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
            DisableAvaloniaDataAnnotationValidation();
            
            desktop.MainWindow = new MainWindow {
                DataContext = mainViewModel
            };
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleView)
        {
            singleView.MainView = new MainView
            {
                DataContext = mainViewModel
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