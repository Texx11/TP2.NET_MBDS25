using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Gauniv.Client.Model;
using Gauniv.Client.Pages;
using Gauniv.Client.Services;
using Gauniv.Network;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Gauniv.Client.ViewModel
{
    [QueryProperty(nameof(Username), nameof(Username))]
    public partial class IndexViewModel : ObservableObject
    {

        private readonly NavigationService _navigationService;

        [ObservableProperty]
        private ObservableCollection<Model.GameDto> gamesDto = new(); // Liste des jeux

        //Liste des jeux non achetés par l'utilisateur
        [ObservableProperty]
        private ObservableCollection<Model.GameDto> gamesDtoNotUser = new(); // Liste des jeux

        //Liste des jeux affichés
        public ObservableCollection<Model.GameDto> DisplayedGames => IsConnected ? GamesDtoNotUser : GamesDto;

        [ObservableProperty]
        private bool isConnected = NetworkService.Instance.TokenMem != null;

        [ObservableProperty]
        private string? username;

        public ICommand BuyCommand { get; }

        private NetworkService _networkService;


        // Constructeur
        public IndexViewModel()
        {
            _networkService = NetworkService.Instance;
            NetworkService.Instance.OnConnected += Instance_OnConnected;
            _navigationService = NavigationService.Instance;
            GetGames(); // Récupération des jeux

            BuyCommand = new RelayCommand<Model.GameDto>(OnBuyClicked);
        }

        private void Instance_OnConnected()
        {
            IsConnected = true;
            UpdateDisplayedGames();

        }
        private void UpdateDisplayedGames()
        {
            DisplayedGames.Clear();
            var source = IsConnected ? GamesDto : GamesDtoNotUser;
            foreach (var game in source)
            {
                DisplayedGames.Add(game);
            }
            Debug.WriteLine(DisplayedGames.Count + " / " + gamesDto.Count + " / " + gamesDtoNotUser.Count);
        }

        [RelayCommand]
        public async Task NavigateToMyGame()
        {
            _navigationService.Navigate<MyGames>(new Dictionary<string, object>());
        }

        [RelayCommand]
        public void LoadGames()
        {
            this.GetGames();
        }

        [RelayCommand]
        public async Task NavigateToLogin()
        {
            _navigationService.Navigate<Login>(new Dictionary<string, object>());
        }

        private void OnBuyClicked(Model.GameDto game)
        {
            //Login automatique pour tester
            /*
            Task.Run(async () =>
            {
                await NetworkService.Instance.Login("user@user.com", "password");
            });*/
            //Send the game to the database through the API
            if (_networkService.TokenMem != null)
            {
                Task.Run(async () =>
                {
                    await NetworkService.Instance.BuyGame(game.Id);
                    await GetGames();
                    // BUG REDIRECTION
                    //_navigationService.Navigate<MyGames>(new Dictionary<string, object>());
                });
            }
            else
            {
                //If the user is not connected, we redirect him to the login page
                this.NavigateToLogin();
            }
        }

        public async Task GetGames()
        {
            try
            {
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    HashSet<int> gameIds = new HashSet<int>();
                    if (IsConnected)
                    {
                        gamesDtoNotUser.Clear();
                        var userGames = await NetworkService.Instance.GetGameUserList();
                        foreach (var g in userGames)
                        {
                            gameIds.Add(g.Id);
                        }
                    }

                    var lastGames = await NetworkService.Instance.GetGameList();
                    gamesDto.Clear();
                    
                    foreach (var g in lastGames)
                    {
                        Model.GameDto gameDto = new Model.GameDto
                        {
                            Id = g.Id,
                            Name = g.Name,
                            Description = g.Description,
                            Price = g.Price,
                        };
                        gamesDto.Add(gameDto);
                        if (IsConnected)
                        {
                            if (!gameIds.Contains(g.Id))  
                            {
                                gamesDtoNotUser.Add(gameDto);
                                gameIds.Add(g.Id);
                            }
                        }
                    }
                });
                
                Debug.WriteLine("Données récupérées avec succès !");
            }
            catch (ApiException ex)
            {
                Debug.WriteLine($"Erreur API : {ex.Message}");
            }
        }

    }
}
