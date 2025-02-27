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
using System.Text;
using System.Threading.Tasks;

namespace Gauniv.Client.ViewModel
{
    public partial class MyGamesViewModel: ObservableObject
    {

        [ObservableProperty]
        private ObservableCollection<Model.GameDto> userGamesDto = new(); // Liste des jeux

        public MyGamesViewModel()
        {
        }

        public async Task GetUserGames()
        {
            try
            {
                Task.Run(async () =>
                {
                    var lastGames = await NetworkService.Instance.GetGameUserList();
                    userGamesDto.Clear();
                    foreach (var g in lastGames)
                    {
                        Model.GameDto gameDto = new Model.GameDto
                        {
                            Id = g.Id,
                            Name = g.Name,
                            Description = g.Description,
                            Price = g.Price,
                        };
                        userGamesDto.Add(gameDto);
                    }
                });
                Console.WriteLine("Données récupérées avec succès !");
            }
            catch (ApiException ex)
            {
                Console.WriteLine($"Erreur API : {ex.Message}");
            }
        }

    }
}
