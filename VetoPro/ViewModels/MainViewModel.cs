using CommunityToolkit.Mvvm.ComponentModel;
using VetoPro.Services;

namespace VetoPro.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly INavigationService _navigationService;

    // Cette propriété contient le ViewModel de la page active (Login ou Dashboard)
    [ObservableProperty]
    private ObservableObject _currentViewModel;

    public MainViewModel(INavigationService navigationService)
    {
        _navigationService = navigationService;
        
        // S'abonner au changement de vue
        _navigationService.CurrentViewChanged += OnCurrentViewChanged;
        
        // Initialiser la vue courante (si déjà définie) ou démarrer sur Login
        if (_navigationService.CurrentView != null)
        {
            CurrentViewModel = _navigationService.CurrentView;
        }
        else
        {
            // Démarrage par défaut
            _navigationService.NavigateTo<LoginPageViewModel>();
        }
    }

    private void OnCurrentViewChanged()
    {
        CurrentViewModel = _navigationService.CurrentView;
    }
}