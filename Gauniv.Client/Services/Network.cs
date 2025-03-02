using CommunityToolkit.Mvvm.ComponentModel;
using Gauniv.Network;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Gauniv.Client.Services
{
    internal partial class NetworkService : ObservableObject
    {

        private readonly WebServeurLinking _apiClient;

        public static NetworkService Instance { get; private set; } = new NetworkService();
        [ObservableProperty]
        private string? tokenMem;
        public HttpClient httpClient;

        public string? CurrentUserId { get; private set; }
        public NetworkService()
        {
            httpClient = new HttpClient();
            tokenMem = null;
            _apiClient = new WebServeurLinking(httpClient);
        }

      
        public async Task<ICollection<GameDto>> GetGameList()
        {
            try
            {
                return await _apiClient.GameAsync(0, 10, null);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while fetching games: {ex.Message}");
                return null;
            }
        }

        
        public async Task<ICollection<GameDto>> GetGameList(int offset, int limit)
        {
            return await _apiClient.GameAsync(offset, limit, null);
        }

        
        public async Task<ICollection<CategoryDto>> GetCategoryList()
        {
            try
            {
                return await _apiClient.CategoryAsync(0, 10);
            } catch (Exception ex)
            {
                Console.WriteLine($"Error while fetching categories: {ex.Message}");
                return null;
            }
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

        public async Task<ICollection<GameDto>> GetGameUserList(int offset, int limit)
        {
            if (string.IsNullOrEmpty(tokenMem))
            {
                return null;
            }
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenMem);
            try
            {
                return await _apiClient.MygamesAsync(offset, limit, null);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while fetching user games: {ex.Message}");
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
                CurrentUserId = username;
                return token.AccessToken;
            }
            return null;
        }

        public async Task<ICollection<GameDto>> GetGameOfCategory(int categoryId)
        {
            try
            {
                return await _apiClient.Category2Async(categoryId);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error while getting game of category: {ex.Message}");
                return null;
            }
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
                Debug.WriteLine($"Error while buying game: {ex.Message}");
            }

            var games = await GetGameUserList();
            if (games != null)
            {
                Debug.WriteLine("Games bought successfully!");
            }
        }

        public async Task<string> DownloadGame(int gameId)
        {
            string fileName = $"game_{gameId}.bin";
            string filePath = Path.Combine(FileSystem.AppDataDirectory, fileName);
            Debug.WriteLine($"File saved at: {filePath}");
            try
            {
                using var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenMem);
                var response = await httpClient.GetAsync($"{_apiClient.BaseUrl}/api/game/download/{gameId}");

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Download failed: {response.ReasonPhrase}");
                    return null;
                }

                await using var responseStream = await response.Content.ReadAsStreamAsync();
                await using var fileStream = File.Create(filePath);
                await responseStream.CopyToAsync(fileStream);
                Debug.WriteLine("Download successful!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Download failed: {ex.Message}");
                return null;
            }

            return filePath;
        }

        public event Action OnConnected;
    }
}
