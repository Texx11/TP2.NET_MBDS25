using CommunityToolkit.Mvvm.ComponentModel;
using Gauniv.Network;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Security;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Store.Preview.InstallControl;
using Windows.Media.Protection.PlayReady;

namespace Gauniv.Client.Services
{
    internal partial class NetworkService : ObservableObject
    {

        private readonly WebServeurLinking _apiClient;

        public static NetworkService Instance { get; private set; } = new NetworkService();
        [ObservableProperty]
        private string? tokenMem;
        public HttpClient httpClient;

        public NetworkService()
        {
            httpClient = new HttpClient();
            tokenMem = null;
            _apiClient = new WebServeurLinking(httpClient);
        }
            
        public async Task<ICollection<GameDto>> GetGameList()
        {
            return await _apiClient.GameAsync(0, 10, null);
        }

        public async Task<ICollection<CategoryDto>> GetCategoryList()
        {
            return await _apiClient.CategoryAsync(0, 10);
        }

        public async Task<ICollection<GameDto>> GetGameUserList()
        {
            if (string.IsNullOrEmpty(tokenMem))
            {
                return null;
            }
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenMem);
            try
            {
                return await _apiClient.MygamesAsync(0, 10, null);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while fetching games: {ex.Message}");
                return null;
            }
        }

        public async Task<string> Login(string username, string password)
        {
            var loginRequest = new LoginRequest
            {
                Email = username,
                Password = password
            };

            var token = await _apiClient.LoginAsync(false, false, loginRequest);
            if (token != null)
            {
                this.tokenMem = token.AccessToken;
                return token.AccessToken;
            }
            return null;
        }

        public async Task BuyGame(int gameId)
        {
            if (string.IsNullOrEmpty(tokenMem))
            {
                return;
            }
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenMem);
            try
            {
                await _apiClient.BuyAsync(gameId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while buying game: {ex.Message}");
            }

            var games = await GetGameUserList();
            if (games != null)
            {
                Console.WriteLine("Games bought successfully!");
            }
        }

        public event Action OnConnected;
    }
}
