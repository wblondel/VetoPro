using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace VetoPro.Services;

public interface INavigationService
{
    // La vue actuellement affichée
    ObservableObject CurrentView { get; }
    
    // Événement pour notifier l'interface graphique que la vue a changé
    event Action CurrentViewChanged;

    // Méthode pour naviguer
    void NavigateTo<T>() where T : ObservableObject;
}