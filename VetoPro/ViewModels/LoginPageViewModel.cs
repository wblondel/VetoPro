using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;
using VetoPro.Contracts.DTOs.Auth; // Pour le LoginDto
using VetoPro.Services; // Pour vos services

namespace VetoPro.ViewModels;

/// <summary>
/// ViewModel pour l'écran de connexion.
/// Gère la saisie de l'utilisateur, l'état de chargement, les messages d'erreur
/// et orchestre le processus de connexion.
/// </summary>
public partial class LoginPageViewModel : ObservableObject
{
    private readonly AuthService _authService;
    private readonly ISecureStorageService _secureStorage;
    private readonly INavigationService _navigationService;

    // --- Propriétés liées à l'UI ---
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(LoginCommand))] // Active/désactive le bouton si vide
    private string _email = "";

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(LoginCommand))] // Active/désactive le bouton si vide
    private string _password = "";

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private bool _isLoading;

    // --- Constructeur ---
    public LoginPageViewModel(
        AuthService authService, 
        ISecureStorageService secureStorage,
        INavigationService navigationService) // <-- Injection
    {
        _authService = authService;
        _secureStorage = secureStorage;
        _navigationService = navigationService;
    }

    // --- Commandes (Actions) ---

    /// <summary>
    /// Commande exécutée lorsque l'utilisateur clique sur le bouton "Login".
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanLogin))]
    private async Task LoginAsync()
    {
        // 1. Réinitialiser l'état
        IsLoading = true;
        ErrorMessage = null;

        var loginData = new LoginDto
        {
            Email = this.Email,
            Password = this.Password
        };

        try
        {
            // 2. Appeler le service d'authentification
            var authResponse = await _authService.LoginAsync(loginData);

            if (authResponse != null)
            {
                // 3. Succès : Stocker les tokens
                await _secureStorage.SetAsync("access_token", authResponse.AccessToken);
                await _secureStorage.SetAsync("refresh_token", authResponse.RefreshToken);
                
                // (Vous pouvez aussi stocker les infos utilisateur (Nom, Rôles)
                // dans un service d'état global si nécessaire)

                // 4. Naviguer vers la page principale de l'application
                _navigationService.NavigateTo<DashboardViewModel>();
            }
            else
            {
                // 5. Échec : Afficher une erreur
                ErrorMessage = "Échec de la connexion. Vérifiez votre e-mail et votre mot de passe.";
            }
        }
        catch (Exception ex)
        {
            // Gérer les erreurs inattendues (ex: réseau)
            ErrorMessage = $"Une erreur est survenue : {ex.Message}";
        }
        finally
        {
            // 6. Finaliser l'état
            IsLoading = false;
        }
    }

    /// <summary>
    /// Logique pour activer/désactiver le bouton de connexion.
    /// </summary>
    private bool CanLogin()
    {
        return !IsLoading && 
               !string.IsNullOrWhiteSpace(Email) && 
               !string.IsNullOrWhiteSpace(Password);
    }
}