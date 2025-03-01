using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Gauniv.Client.Model;
using Gauniv.Client.Services;
using Gauniv.Network;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Gauniv.Client.ViewModel
{
    public partial class MyGamesViewModel : ObservableObject
    {
        private NetworkService _networkService;

        [ObservableProperty]
        private ObservableCollection<Model.GameDto> userGamesDto = new();

        // Offset et limit pour la pagination
        private int offset = 0;
        private int limit = 4;

        [ObservableProperty]
        private bool isMoreDataAvailable = true;

        public MyGamesViewModel()
        {
            _networkService = NetworkService.Instance;
            userGamesDto.Clear();
        }

        [RelayCommand]
        public void LoadUserGames()
        {
            // On remet l’offset à 0 seulement si c’est la première fois
            offset = 0;
            isMoreDataAvailable = true;
            UserGamesDto.Clear();
            GetUserGames();
        }

        [RelayCommand]
        public void LoadMoreUserGames()
        {
            GetUserGames();
        }

        private async void GetUserGames()
        {
            if (_networkService.TokenMem == null)
            {
                return;
            }
            try
            {
                var lastGames = await _networkService.GetGameUserList(offset, limit);
                if (lastGames == null || lastGames.Count == 0)
                {
                    // S’il n’y a plus rien, on arrête
                    IsMoreDataAvailable = false;
                    return;
                }
                offset += lastGames.Count;

                // Ajoute seulement les nouveaux jeux
                foreach (var g in lastGames)
                {
                    if (!UserGamesDto.Any(game => game.Id == g.Id))
                    {
                        var gameDto = new Model.GameDto
                        {
                            Id = g.Id,
                            Name = g.Name,
                            Description = g.Description,
                            Price = g.Price,
                        };
                        UserGamesDto.Add(gameDto);
                    }
                }

                // Si on a reçu moins que 'limit', c’est probablement la fin
                if (lastGames.Count < limit)
                {
                    IsMoreDataAvailable = false;
                }

                Console.WriteLine("Données récupérées avec succès !");
            }
            catch (ApiException ex)
            {
                Console.WriteLine($"Erreur API : {ex.Message}");
            }
        }
    }
}
