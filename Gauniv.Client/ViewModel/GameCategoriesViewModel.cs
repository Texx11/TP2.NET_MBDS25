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
    [QueryProperty(nameof(CategoryId), nameof(CategoryId))]
    [QueryProperty(nameof(CategoryName), nameof(CategoryName))]
    public partial class GameCategoriesViewModel : ObservableObject
    {

        [ObservableProperty]
        private ObservableCollection<Model.GameDto> gamesCategory = new();

        [ObservableProperty]
        private int categoryId;

        [ObservableProperty]
        private String? categoryName;



        private readonly NavigationService _navigationService;
        // Constructeur
        public GameCategoriesViewModel()
        {
            _navigationService = NavigationService.Instance;

        }
        partial void OnCategoryIdChanged(int value)
        {
            if (value != 0)
            {
                GetGameCategories(value);
            }
        }

        [RelayCommand]
        public async Task BackToCategories()
        {
            _navigationService.Navigate<Pages.Categories>(new Dictionary<string, object> { });
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
            });
        }
    }
}
