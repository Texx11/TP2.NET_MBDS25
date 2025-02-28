using Gauniv.Client.ViewModel;

namespace Gauniv.Client.Pages;

public partial class MyGames : ContentPage
{
    private MyGamesViewModel viewModel;
    public MyGames()
	{
		InitializeComponent();
        viewModel = new MyGamesViewModel();
        var games = viewModel.UserGamesDto;
        BindingContext = viewModel;
    }
}