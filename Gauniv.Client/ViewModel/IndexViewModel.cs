using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Gauniv.Client.Model;
using Gauniv.Client.Pages;
using Gauniv.Client.Services;
using Gauniv.Network;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

// Alias pour désambiguïser GameDto provenant du réseau
using NetworkGameDto = Gauniv.Network.GameDto;

namespace Gauniv.Client.ViewModel
{
    [QueryProperty(nameof(Username), nameof(Username))]
    public partial class IndexViewModel : ObservableObject
    {
        private readonly NavigationService _navigationService;
        private readonly NetworkService _networkService;

        // Données chargées depuis le serveur (converties en GameItem)
        private List<GameItem> _allGames = new();

        // Pagination
        private int limit = 4;
        private int offset = 0;

        [ObservableProperty]
        private bool isMoreDataAvailable = true;

        [ObservableProperty]
        private bool isConnected = false;

        [ObservableProperty]
        private string? username;

        // ---- Filtres ----
        [ObservableProperty]
        private string searchName; // champ "Nom"

        [ObservableProperty]
        private float? minPrice;

        [ObservableProperty]
        private float? maxPrice;

        [ObservableProperty]
        private string selectedCategory; // Catégorie choisie

        [ObservableProperty]
        private bool showPossessed; // Filtrer pour n'afficher que les jeux possédés

        // Liste des catégories pour le Picker
        [ObservableProperty]
        private ObservableCollection<string> categoriesList = new();

        // Collection affichée (résultat des filtres)
        [ObservableProperty]
        private ObservableCollection<GameItem> displayedGames = new();

        // Commandes
        public ICommand BuyCommand { get; }
        public ICommand LoadGamesCommand { get; }
        public ICommand LoadMoreCommand { get; }
        public ICommand NavigateToLoginCommand { get; }
        public ICommand ApplyFiltersCommand { get; }
        public ICommand ResetFiltersCommand { get; }

        public IndexViewModel()
        {
            _navigationService = NavigationService.Instance;
            _networkService = NetworkService.Instance;

            // On considère connecté si le token existe
            IsConnected = !string.IsNullOrEmpty(_networkService.TokenMem);

            BuyCommand = new RelayCommand<GameItem>(OnBuyClicked);
            LoadGamesCommand = new RelayCommand(OnInitialLoad);
            LoadMoreCommand = new RelayCommand(OnLoadMore);
            NavigateToLoginCommand = new RelayCommand(NavigateToLogin);
            ApplyFiltersCommand = new RelayCommand(ApplyFilters);
            ResetFiltersCommand = new RelayCommand(ResetFilters);

            _networkService.OnConnected += Instance_OnConnected;
        }

        private async void Instance_OnConnected()
        {
            IsConnected = true;
            offset = 0;
            _allGames.Clear();
            DisplayedGames.Clear();
            await LoadCategoriesAsync();
            OnInitialLoad();
        }

        private async void OnInitialLoad()
        {
            offset = 0;
            IsMoreDataAvailable = true;
            _allGames.Clear();
            DisplayedGames.Clear();
            // Réinitialisation des filtres par défaut
            SearchName = string.Empty;
            MinPrice = null;
            MaxPrice = null;
            SelectedCategory = "Toutes";
            ShowPossessed = false;

            await LoadCategoriesAsync();
            await LoadMoreInternal();
        }

        private async void OnLoadMore()
        {
            await LoadMoreInternal();
        }

        private void ResetFilters()
        {
            SearchName = string.Empty;
            MinPrice = null;
            MaxPrice = null;
            SelectedCategory = "Toutes";
            ShowPossessed = false;
            ApplyFilters();
        }

        private async Task LoadMoreInternal()
        {
            try
            {
                // Récupération d'un bloc de jeux depuis l'API (retourne NetworkGameDto)
                var lastGamesNetwork = await _networkService.GetGameList(offset, limit);
                if (lastGamesNetwork == null || lastGamesNetwork.Count == 0)
                {
                    IsMoreDataAvailable = false;
                    return;
                }
                offset += lastGamesNetwork.Count;
                // Conversion des objets réseau en GameItem
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
                    _allGames.Add(gameItem);
                }
                ApplyFilters();

                if (lastGamesNetwork.Count < limit)
                {
                    IsMoreDataAvailable = false;
                }
            }
            catch (ApiException ex)
            {
                Debug.WriteLine("Erreur API: " + ex.Message);
            }
        }

        private async Task LoadCategoriesAsync()
        {
            try
            {
                categoriesList.Clear();
                categoriesList.Add("Toutes");
                var catDtos = await _networkService.GetCategoryList();
                if (catDtos != null)
                {
                    foreach (var c in catDtos)
                    {
                        if (!string.IsNullOrWhiteSpace(c.Name))
                            categoriesList.Add(c.Name);
                    }
                }
                SelectedCategory = "Toutes";
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Impossible de charger les catégories: " + ex.Message);
            }
        }

        private async void ApplyFilters()
        {
            var filtered = _allGames.AsEnumerable();

            // Filtre par nom (conversion en minuscules et suppression des espaces)
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
            if (IsConnected && ShowPossessed)
            {
                var userGames = await _networkService.GetGameUserList();
                var ownedIds = new HashSet<int>(userGames?.Select(x => x.Id) ?? new List<int>());
                filtered = filtered.Where(g => ownedIds.Contains(g.Id));
            }
            var finalList = filtered.ToList();

            DisplayedGames.Clear();
            foreach (var g in finalList)
            {
                DisplayedGames.Add(g);
            }
        }

        private void OnBuyClicked(GameItem game)
        {
            if (string.IsNullOrEmpty(_networkService.TokenMem))
            {
                NavigateToLogin();
                return;
            }
            Task.Run(async () =>
            {
                await _networkService.BuyGame(game.Id);
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    ApplyFilters();
                });
            });
        }

        private void NavigateToLogin()
        {
            _navigationService.Navigate<Login>(new Dictionary<string, object>());
        }
    }
}
