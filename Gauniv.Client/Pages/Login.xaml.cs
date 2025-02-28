using Gauniv.Client.ViewModel;

namespace Gauniv.Client.Pages;

public partial class Login : ContentPage
{
    private LoginViewModel viewModel;

    public Login()
	{
		InitializeComponent();
        viewModel = new LoginViewModel();
        BindingContext = viewModel;
    }
}