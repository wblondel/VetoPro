using CommunityToolkit.Mvvm.ComponentModel;

namespace VetoPro.ViewModels;

public partial class DashboardViewModel : ObservableObject
{
    // Vous pouvez ajouter ici des propriétés pour le tableau de bord
    // (ex: "Bienvenue Dr. Smith", "3 RDV aujourd'hui", etc.)
    [ObservableProperty]
    private string _welcomeMessage = "Bienvenue sur VetoPro !";
}