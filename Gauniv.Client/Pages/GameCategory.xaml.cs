
using Gauniv.Client.Model;
using Gauniv.Client.ViewModel;
using Microsoft.Maui.Controls;
using System.Windows.Input;

namespace Gauniv.Client.Pages
{


    public partial class GameCategory : ContentPage
    {
        private CategoriesViewModel viewModel;

        public GameCategory()
        {
            InitializeComponent();
            viewModel = new CategoriesViewModel();
            BindingContext = viewModel;
        }
    }
}