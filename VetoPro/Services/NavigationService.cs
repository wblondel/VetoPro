using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace VetoPro.Services;

public class NavigationService : INavigationService
{
    private readonly IServiceProvider _serviceProvider;
    private ObservableObject _currentView;

    public NavigationService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public ObservableObject CurrentView
    {
        get => _currentView;
        private set
        {
            _currentView = value;
            CurrentViewChanged?.Invoke();
        }
    }

    public event Action CurrentViewChanged;

    public void NavigateTo<T>() where T : ObservableObject
    {
        // On utilise le ServiceProvider pour créer le ViewModel demandé.
        // Cela permet de résoudre automatiquement ses dépendances (Services, etc.)
        var newViewModel = _serviceProvider.GetRequiredService<T>();
        CurrentView = newViewModel;
    }
}