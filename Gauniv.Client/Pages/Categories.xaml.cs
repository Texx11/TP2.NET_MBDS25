
using Gauniv.Client.Model;
using Gauniv.Client.ViewModel;
using Microsoft.Maui.Controls;

namespace Gauniv.Client.Pages
{


   public partial class Categories : ContentPage
    {
        private CategoriesViewModel viewModel;
        public Categories()
        {
            InitializeComponent();
            viewModel = new CategoriesViewModel();
            BindingContext = viewModel;
        }
    }
}