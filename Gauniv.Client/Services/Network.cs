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
        private string token;
        public HttpClient httpClient;

        public NetworkService()
        {
            httpClient = new HttpClient();
            Token = null;
            _apiClient = new WebServeurLinking(httpClient);
        }
            
        public async Task<ICollection<GameDto>> GetGameList()
        {
            return await _apiClient.GameAsync(0, 10, null);
        }

        public event Action OnConnected;

    }
}
