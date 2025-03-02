
using Gauniv.Client.Model;
using Gauniv.Client.ViewModel;
using Microsoft.Maui.Controls;
using System.Windows.Input;

namespace Gauniv.Client.Pages
{


    public partial class GameCategory : ContentPage
    {
        private GameCategoriesViewModel viewModel;

        public GameCategory()
        {
            InitializeComponent();
            viewModel = new GameCategoriesViewModel();
            BindingContext = viewModel;
        }
    }
}