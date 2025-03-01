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

// Alias pour désambiguïser GameDto provenant du réseau
using NetworkGameDto = Gauniv.Network.GameDto;

namespace Gauniv.Client.ViewModel
{
    public partial class MyGamesViewModel : ObservableObject
    {
        private readonly NetworkService _networkService;

        // On conserve tous les jeux possédés chargés (convertis en GameItem)
        private List<GameItem> _allUserGames = new();

        private int offset = 0;
        private int limit = 4;

        [ObservableProperty]
        private bool isMoreDataAvailable = true;

        // Propriétés de filtres
        [ObservableProperty]
        private string searchName;

        [ObservableProperty]
        private float? minPrice;

        [ObservableProperty]
        private float? maxPrice;

        [ObservableProperty]
        private string selectedCategory;

        // Liste des catégories pour le Picker
        [ObservableProperty]
        private ObservableCollection<string> categoriesList = new();

        // Résultat filtré affiché (utilise GameItem)
        [ObservableProperty]
        private ObservableCollection<GameItem> filteredUserGames = new();

        public MyGamesViewModel()
        {
            _networkService = NetworkService.Instance;
            _allUserGames.Clear();
        }

        [RelayCommand]
        public void LoadUserGames()
        {
            offset = 0;
            isMoreDataAvailable = true;
            _allUserGames.Clear();
            FilteredUserGames.Clear();

            SearchName = string.Empty;
            MinPrice = null;
            MaxPrice = null;
            SelectedCategory = null;

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
            catch (Exception ex)
            {
                Debug.WriteLine("Erreur lors du chargement des catégories: " + ex.Message);
            }
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
                    isMoreDataAvailable = false;
                    return;
                }
                offset += lastGamesNetwork.Count;
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
                    _allUserGames.Add(gameItem);
                }
                ApplyFilterInternal();
                if (lastGamesNetwork.Count < limit)
                {
                    isMoreDataAvailable = false;
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
                filtered = filtered.Where(g => !string.IsNullOrEmpty(g.Name) &&
                    g.Name.Contains(SearchName, System.StringComparison.OrdinalIgnoreCase));
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
    }
}
