using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Gauniv.Client.Model;
using Gauniv.Client.Services;
using Gauniv.Network;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.Storage;

using NetworkGameDto = Gauniv.Network.GameDto;

namespace Gauniv.Client.ViewModel
{
    public partial class MyGamesViewModel : ObservableObject
    {
        // Clé pour stocker les IDs des jeux téléchargés
        private string DownloadedGamesKey => $"DownloadedGames_{_networkService.CurrentUserId}";

        private readonly NetworkService _networkService;
        private List<GameItem> _allUserGames = new();

        private int offset = 0;
        private int limit = 4;

        [ObservableProperty]
        private bool isMoreDataAvailable = true;

        // Propriétés de filtres
        [ObservableProperty]
        private string searchName = string.Empty;

        [ObservableProperty]
        private float? minPrice;

        [ObservableProperty]
        private float? maxPrice;

        [ObservableProperty]
        private string selectedCategory = "Toutes";

        // Liste des catégories pour le Picker
        [ObservableProperty]
        private ObservableCollection<string> categoriesList = new();

        // Liste des jeux filtrés affichés
        [ObservableProperty]
        private ObservableCollection<GameItem> filteredUserGames = new();

        // Commande pour réinitialiser les filtres
        public ICommand ResetFiltersCommand { get; }

        public MyGamesViewModel()
        {
            _networkService = NetworkService.Instance;
            _allUserGames.Clear();
            ResetFiltersCommand = new RelayCommand(ResetFilters);
        }

        [RelayCommand]
        public void LoadUserGames()
        {
            offset = 0;
            IsMoreDataAvailable = true;
            _allUserGames.Clear();
            FilteredUserGames.Clear();

            // Réinitialisation des filtres par défaut
            SearchName = string.Empty;
            MinPrice = null;
            MaxPrice = null;
            SelectedCategory = "Toutes";

            Task.Run(async () => await LoadCategoriesAsync());
            GetUserGamesChunk();
        }

        [RelayCommand]
        public void LoadMoreUserGames()
        {
            GetUserGamesChunk();
        }

        [RelayCommand]
        public void ApplyFilters()
        {
            ApplyFilterInternal();
        }

        private async Task LoadCategoriesAsync()
        {
            try
            {
                var cats = await _networkService.GetCategoryList();
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    categoriesList.Clear();
                    categoriesList.Add("Toutes");
                    if (cats != null)
                    {
                        foreach (var c in cats)
                        {
                            if (!string.IsNullOrEmpty(c.Name))
                                categoriesList.Add(c.Name);
                        }
                    }
                    SelectedCategory = "Toutes";
                });
            }
            catch (System.Exception ex)
            {
                Debug.WriteLine("Erreur lors du chargement des catégories: " + ex.Message);
            }
        }

        private void ResetFilters()
        {
            SearchName = string.Empty;
            MinPrice = null;
            MaxPrice = null;
            SelectedCategory = "Toutes";
            ApplyFilterInternal();
        }

        private async void GetUserGamesChunk()
        {
            if (_networkService.TokenMem == null)
            {
                return;
            }
            try
            {
                var lastGamesNetwork = await _networkService.GetGameUserList(offset, limit);
                if (lastGamesNetwork == null || lastGamesNetwork.Count == 0)
                {
                    IsMoreDataAvailable = false;
                    return;
                }
                offset += lastGamesNetwork.Count;

                // Récupérer la liste persistante des IDs téléchargés
                var downloadedList = Preferences.Get(DownloadedGamesKey, "");
                var downloadedIds = downloadedList.Split(new char[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries);

                foreach (NetworkGameDto netGame in lastGamesNetwork)
                {
                    var gameItem = new GameItem
                    {
                        Id = netGame.Id,
                        Name = netGame.Name,
                        Description = netGame.Description,
                        Price = netGame.Price,
                        Categories = new List<string>(netGame.Categories)
                    };
                    // Si l'ID du jeu figure dans la liste sauvegardée, on marque comme téléchargé
                    if (downloadedIds.Contains(gameItem.Id.ToString()))
                    {
                        gameItem.IsDownloaded = true;
                    }
                    _allUserGames.Add(gameItem);
                }
                ApplyFilterInternal();
                if (lastGamesNetwork.Count < limit)
                {
                    IsMoreDataAvailable = false;
                }
            }
            catch (ApiException ex)
            {
                Debug.WriteLine($"Erreur API : {ex.Message}");
            }
        }

        private void ApplyFilterInternal()
        {
            var filtered = _allUserGames.AsEnumerable();
            if (!string.IsNullOrWhiteSpace(SearchName))
            {
                string searchLower = SearchName.Trim().ToLower();
                filtered = filtered.Where(g => !string.IsNullOrEmpty(g.Name) &&
                    g.Name.Trim().ToLower().Contains(searchLower));
            }
            if (MinPrice.HasValue)
            {
                filtered = filtered.Where(g => g.Price >= MinPrice.Value);
            }
            if (MaxPrice.HasValue)
            {
                filtered = filtered.Where(g => g.Price <= MaxPrice.Value);
            }
            if (!string.IsNullOrWhiteSpace(SelectedCategory) && SelectedCategory != "Toutes")
            {
                filtered = filtered.Where(g => g.Categories.Any(cat => cat.Equals(SelectedCategory, System.StringComparison.OrdinalIgnoreCase)));
            }
            var finalList = filtered.ToList();
            MainThread.BeginInvokeOnMainThread(() =>
            {
                FilteredUserGames.Clear();
                foreach (var g in finalList)
                {
                    FilteredUserGames.Add(g);
                }
            });
        }

        // Commande pour simuler le téléchargement d'un jeu et mettre à jour les préférences
        [RelayCommand]
        private async Task Download(GameItem item)
        {
            if (item == null)
                return;

            // Simulation d'un téléchargement (1 seconde)
            await Task.Delay(1000);
            item.IsDownloaded = true;

            // Mise à jour persistante : ajouter l'ID du jeu si non présent
            var downloadedList = Preferences.Get(DownloadedGamesKey, "");
            var downloadedIds = downloadedList.Split(new char[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries).ToList();
            if (!downloadedIds.Contains(item.Id.ToString()))
            {
                downloadedIds.Add(item.Id.ToString());
                Preferences.Set(DownloadedGamesKey, string.Join(",", downloadedIds));
            }
        }

        // Commande pour simuler le lancement d'un jeu
        [RelayCommand]
        private async Task Play(GameItem item)
        {
            if (item == null)
                return;

            await App.Current.MainPage.DisplayAlert("Jouer",
                $"Lancement du jeu : {item.Name}", "OK");
        }

        // Commande pour simuler la suppression d'un téléchargement et mettre à jour les préférences
        [RelayCommand]
        private async Task Delete(GameItem item)
        {
            if (item == null)
                return;

            item.IsDownloaded = false;
            await Task.Delay(200);

            // Mise à jour persistante : retirer l'ID du jeu téléchargé
            var downloadedList = Preferences.Get(DownloadedGamesKey, "");
            var downloadedIds = downloadedList.Split(new char[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries).ToList();
            if (downloadedIds.Contains(item.Id.ToString()))
            {
                downloadedIds.Remove(item.Id.ToString());
                Preferences.Set(DownloadedGamesKey, string.Join(",", downloadedIds));
            }
        }
    }
}
