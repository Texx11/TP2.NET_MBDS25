using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Gauniv.Client.Model;
using Gauniv.Client.Pages;
using Gauniv.Client.Services;
using Gauniv.Network;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;

namespace Gauniv.Client.ViewModel
{
    [QueryProperty(nameof(Username), nameof(Username))]
    public partial class IndexViewModel : ObservableObject
    {
        private readonly NavigationService _navigationService;
        private NetworkService _networkService;

        // Liste complète de tous les jeux chargés (quand on est pas connecté)
        [ObservableProperty]
        private ObservableCollection<Model.GameDto> gamesDto = new();

        // Liste des jeux non achetés (si on est connecté)
        [ObservableProperty]
        private ObservableCollection<Model.GameDto> gamesDtoNotUser = new();

        // Détermine combien d’éléments on demande par requête
        private int limit = 4;

        // Offset qui s’incrémente au fur et à mesure
        private int offset = 0;

        // Indique s’il y a potentiellement d’autres données à charger
        [ObservableProperty]
        private bool isMoreDataAvailable = true;

        [ObservableProperty]
        private bool isConnected = false;

        [ObservableProperty]
        private string? username;

     
        public ObservableCollection<Model.GameDto> DisplayedGames => IsConnected ? GamesDtoNotUser : GamesDto;

        public ICommand BuyCommand { get; }
        public ICommand LoadGamesCommand { get; }
        public ICommand LoadMoreCommand { get; }
        public ICommand NavigateToLoginCommand { get; }

        public IndexViewModel()
        {
            _networkService = NetworkService.Instance;
            _navigationService = NavigationService.Instance;

            // On considère qu’on est connecté si on a un token (tokenMem != null)
            IsConnected = !string.IsNullOrEmpty(_networkService.TokenMem);

            // Instanciation des commandes
            BuyCommand = new RelayCommand<Model.GameDto>(OnBuyClicked);
            LoadGamesCommand = new RelayCommand(OnInitialLoad);
            LoadMoreCommand = new RelayCommand(OnLoadMore);
            NavigateToLoginCommand = new RelayCommand(NavigateToLogin);

            // Écoute l’évènement du NetworkService quand la connexion se fait
            _networkService.OnConnected += Instance_OnConnected;
        }

        private void Instance_OnConnected()
        {
            IsConnected = true;
            offset = 0;
            GamesDto.Clear();
            GamesDtoNotUser.Clear();
            OnInitialLoad();
        }
        private void OnInitialLoad()
        {
            offset = 0;
            IsMoreDataAvailable = true;
            GamesDto.Clear();
            GamesDtoNotUser.Clear();
            GetGames();
        }

        private async void OnLoadMore()
        {
            GetGames();
        }

        private async void GetGames()
        {
            // On appelle l’API différemment selon qu’on est connecté ou non
            if (IsConnected)
            {
                // Récupérer la liste des jeux POSSEDÉS
                var userGames = await _networkService.GetGameUserList(0, 9999);
                var ownedIds = new HashSet<int>(userGames?.Select(g => g.Id) ?? new List<int>());

                // Charger la suite des jeux "globaux"
                var lastGames = await _networkService.GetGameList(offset, limit);
                if (lastGames == null || lastGames.Count == 0)
                {
                    // S’il n’y a plus rien, on arrête
                    IsMoreDataAvailable = false;
                    return;
                }
                offset += lastGames.Count;
                // On ajoute uniquement ceux que l’utilisateur ne possède pas
                foreach (var g in lastGames)
                {
                    // On remplit GamesDtoNotUser
                    if (!ownedIds.Contains(g.Id))
                    {
                        GamesDtoNotUser.Add(new Model.GameDto
                        {
                            Id = g.Id,
                            Name = g.Name,
                            Description = g.Description,
                            Price = g.Price
                        });
                    }
                }
                // Si on a reçu moins que 'limit', c’est probablement la fin
                if (lastGames.Count < limit)
                {
                    IsMoreDataAvailable = false;
                }
            }
            else
            {
                // Non connecté
                var lastGames = await _networkService.GetGameList(offset, limit);
                if (lastGames == null || lastGames.Count == 0)
                {
                    IsMoreDataAvailable = false;
                    return;
                }
                offset += lastGames.Count;

                foreach (var g in lastGames)
                {
                    GamesDto.Add(new Model.GameDto
                    {
                        Id = g.Id,
                        Name = g.Name,
                        Description = g.Description,
                        Price = g.Price,
                    });
                }
                if (lastGames.Count < limit)
                {
                    IsMoreDataAvailable = false;
                }
            }
        }

        private void OnBuyClicked(Model.GameDto game)
        {
            // Si on n’est pas connecté, on redirige vers la page Login
            if (string.IsNullOrEmpty(_networkService.TokenMem))
            {
                NavigateToLogin();
                return;
            }

            // Achat du jeu
            Task.Run(async () =>
            {
                await _networkService.BuyGame(game.Id);
                // On rafraîchit la liste => retire ce jeu de la liste "non possédés"
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    GamesDtoNotUser.Remove(game);
                });
            });
        }

        private void NavigateToLogin()
        {
            _navigationService.Navigate<Login>(new Dictionary<string, object>());
        }
    }
}
