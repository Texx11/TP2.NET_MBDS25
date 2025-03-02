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
    public partial class CategoriesViewModel : ObservableObject
    {


        [ObservableProperty]
        private ObservableCollection<Model.CategoryDto> categoryDtos = new(); // Liste des categories7

        [ObservableProperty]
        private ObservableCollection<Model.GameDto> gamesCategory = new();


        private readonly NavigationService _navigationService;
        // Constructeur
        public CategoriesViewModel()
        {
            _navigationService = NavigationService.Instance;
            GetCategories(); // Récupération des jeux
        }
        public async Task GetCategories()
        {
            try
            {
                Task.Run(async () =>
                {
                    var lastCategoryDtos = await NetworkService.Instance.GetCategoryList();
                    categoryDtos.Clear();
                    foreach (var c in lastCategoryDtos)
                    {
                        Model.CategoryDto categoryDto = new Model.CategoryDto
                        {
                            Id = c.Id,
                            Name = c.Name
                        };
                        categoryDtos.Add(categoryDto);
                    }
                });
                var test = categoryDtos;
                Console.WriteLine("Données récupérées avec succès !");
            }
            catch (ApiException ex)
            {
                Console.WriteLine($"Erreur API : {ex.Message}");
            }
        }

        public async Task GetGameCategories(int categoryId)
        {
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                var lastGameCategory = await NetworkService.Instance.GetGameOfCategory(categoryId);
                foreach (var g in lastGameCategory)
                {
                    Model.GameDto gameDto = new Model.GameDto
                    {
                        Id = g.Id,
                        Name = g.Name,
                        Description = g.Description,
                        Price = g.Price
                    };
                    gamesCategory.Add(gameDto);
                }
                //Navigate to GameCategory
                _navigationService.Navigate<GameCategory>(new Dictionary<string, object>());
            });
        }
    }
}
