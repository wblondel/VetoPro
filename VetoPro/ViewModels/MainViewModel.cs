using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using VetoPro.Contracts.DTOs;
using VetoPro.Services;

namespace VetoPro.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private readonly ISpeciesService _speciesService;
    private ObservableCollection<SpeciesDto> _species;
    private ObservableCollection<BreedDto> _breeds;
    private SpeciesDto? _selectedSpecies;
    private bool _isLoading;
    private string _errorMessage;

    public MainViewModel(ISpeciesService speciesService)
    {
        _speciesService = speciesService;
        _species = new ObservableCollection<SpeciesDto>();
        _breeds = new ObservableCollection<BreedDto>();
        _errorMessage = string.Empty;

        // Load data when ViewModel is created
        _ = LoadSpeciesAsync();
    }

    public ObservableCollection<SpeciesDto> Species
    {
        get => _species;
        set => SetProperty(ref _species, value);
    }

    public ObservableCollection<BreedDto> Breeds
    {
        get => _breeds;
        set => SetProperty(ref _breeds, value);
    }

    public SpeciesDto? SelectedSpecies
    {
        get => _selectedSpecies;
        set
        {
            if (SetProperty(ref _selectedSpecies, value) && value != null)
            {
                // When a species is selected, load its breeds
                _ = LoadBreedsForSpeciesAsync(value.Id);
            }
        }
    }

    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    /// <summary>
    /// Loads all species from the API
    /// </summary>
    public async Task LoadSpeciesAsync()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            var speciesList = await _speciesService.GetAllSpeciesAsync();
            
            Species.Clear();
            foreach (var species in speciesList)
            {
                Species.Add(species);
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Erreur lors du chargement des espèces: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Loads breeds for a specific species
    /// </summary>
    public async Task LoadBreedsForSpeciesAsync(Guid speciesId)
    {
        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            var breedsList = await _speciesService.GetBreedsForSpeciesAsync(speciesId);
            
            Breeds.Clear();
            foreach (var breed in breedsList)
            {
                Breeds.Add(breed);
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Erreur lors du chargement des races: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }
}