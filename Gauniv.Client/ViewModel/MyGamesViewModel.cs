using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Gauniv.Client.Model;
using Gauniv.Client.Pages;
using Gauniv.Client.Services;
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
        private ObservableCollection<Game> userGames = new(); // Liste des jeux possédés par l'utilisateur

        [ObservableProperty]
        private ObservableCollection<Category> categories = new(); // Liste des catégories des jeux correspondants

        public MyGamesViewModel()
        {
            LoadUserGames();
            LoadCategories();
        }

        public void LoadUserGames()
        {
            userGames = new ObservableCollection<Game>
            {
                new Game { Id = 1, Name = "Game 1", Description = "Description 1", Price = 10.0f },
                new Game { Id = 2, Name = "Game 2", Description = "Description 2", Price = 20.0f },
                new Game { Id = 3, Name = "Game 3", Description = "Description 3", Price = 30.0f },
                new Game { Id = 4, Name = "Game 4", Description = "Description 4", Price = 40.0f }
            };
        }

        public void LoadCategories() {
            categories = new ObservableCollection<Category>
            {
                new Category { Id = 1, Name = "Category 1" },
                new Category { Id = 2, Name = "Category 2" }
            };
        }

    }
}
