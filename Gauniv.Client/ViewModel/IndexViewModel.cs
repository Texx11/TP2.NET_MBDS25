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
    public partial class IndexViewModel : ObservableObject
    {


        [ObservableProperty]
        private ObservableCollection<Model.GameDto> gamesDto = new(); // Liste des jeux

        public ICommand BuyCommand { get; }


        // Constructeur
        public IndexViewModel()
        {
            GetGames(); // Récupération des jeux

            BuyCommand = new RelayCommand<Model.GameDto>(OnBuyClicked);

        }
        private void OnBuyClicked(Model.GameDto game)
        {
            //Login automatique pour tester
            Task.Run(async () =>
            {
                await NetworkService.Instance.Login("user@user.com", "password");
            });
            // Logique d'achat ici
            Console.WriteLine($"Achat du jeu: {game.Name}");
            var gameRecup = game;
            //Send the game to the database through the API
            Task.Run(async () =>
            {
                await NetworkService.Instance.BuyGame(gameRecup.Id);
            });

        }

        public async Task GetGames()
        {
            try
            {
                Task.Run(async () =>
                {
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
                    }
                });

                Console.WriteLine("Données récupérées avec succès !");
            }
            catch (ApiException ex)
            {
                Console.WriteLine($"Erreur API : {ex.Message}");
            }
        }

    }
}
