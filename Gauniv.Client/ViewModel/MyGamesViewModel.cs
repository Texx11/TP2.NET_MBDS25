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
using System.Text;
using System.Threading.Tasks;

namespace Gauniv.Client.ViewModel
{
    public partial class MyGamesViewModel: ObservableObject
    {


        [ObservableProperty]
        private ObservableCollection<Model.GameDto> userGamesDto = new(); // Liste des jeux

        private NetworkService _networkService;
        public MyGamesViewModel()
        {
            _networkService = NetworkService.Instance;
            this.UserGamesDto.Clear();
            GetUserGames(); // Récupération des jeux
        }

        public async Task GetUserGames()
        {
            //Login factice pour l'instant
            //string response = await NetworkService.Instance.Login("user@user.com", "password");
            this.UserGamesDto.Clear();
            if (_networkService.TokenMem == null)
            {
                return;
            }
            try
            {
                // await MainThread.InvokeOnMainThreadAsync(() =>
                var lastGames = await _networkService.GetGameUserList();
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    foreach (var g in lastGames)
                    {
                        if (!UserGamesDto.Any(game => game.Id == g.Id))
                        {
                            Model.GameDto gameDto = new Model.GameDto
                            {
                                Id = g.Id,
                                Name = g.Name,
                                Description = g.Description,
                                Price = g.Price,
                            };

                            UserGamesDto.Add(gameDto);
                        }
                    }
                });
                Console.WriteLine("Données récupérées avec succès !");
            }
            catch (ApiException ex)
            {
                Console.WriteLine($"Erreur API : {ex.Message}");
            }
        }

        [RelayCommand]
        public void LoadUserGames()
        {
            if (_networkService.TokenMem != null)
            {
                this.GetUserGames();
            }
        }
    }
}
