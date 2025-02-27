
using Gauniv.Client.Model;
using Gauniv.Client.ViewModel;
using Microsoft.Maui.Controls;

namespace Gauniv.Client.Pages
{


    public partial class Index : ContentPage
    {
        private IndexViewModel viewModel;
        public Index()
        {
            InitializeComponent();
            viewModel = new IndexViewModel();
            BindingContext = viewModel;
        }
    }
}