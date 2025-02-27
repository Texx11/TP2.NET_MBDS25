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

        [ObservableProperty]
        private ObservableCollection<CategoryDto> categoryDto = new();

        [ObservableProperty]
        private ObservableCollection<Game> games = new(); // Liste des jeux

        [ObservableProperty]
        private ObservableCollection<Category> categories = new(); // Liste des catégories

        // Constructeur
        public IndexViewModel()
        {
            GetGames(); // Récupération des jeux
            LoadCategories(); // Chargement initial des catégories
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

        public async Task GetCategories()
        {
           
        }

        // Fonction pour charger les jeux
        private void LoadGames()
        {
            games = new ObservableCollection<Game>
            {
                new Game { Id = 1, Name = "Game 1", Description = "Description 1", Price = 10.0f },
                new Game { Id = 2, Name = "Game 2", Description = "Description 2", Price = 20.0f },
                new Game { Id = 3, Name = "Game 3", Description = "Description 3", Price = 30.0f },
                new Game { Id = 4, Name = "Game 4", Description = "Description 4", Price = 40.0f }
            };
        }

        // Fonction pour charger les catégories
        private void LoadCategories()
        {
            categories = new ObservableCollection<Category>
            {
                new Category { Id = 1, Name = "Category 1" },
                new Category { Id = 2, Name = "Category 2" }
            };
        }


        // Commande pour acheter un jeu
        [RelayCommand]
        private void buyGame(Game game)
        {
            Debug.WriteLine("Achat test");

            if (game != null)
            {
                //Navigation to MyGames.xaml
                Debug.WriteLine("Achat du jeu " + game.Name);

            }
        }
    }
}
