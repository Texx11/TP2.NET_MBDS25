using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Gauniv.Client.Pages;
using Gauniv.Client.Model;
using Gauniv.Client.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gauniv.Client.ViewModel
{
    partial class LoginViewModel : ObservableObject
    {
        [ObservableProperty]
        private String? username;

        [ObservableProperty]
        private String? password;

        private readonly NavigationService _navigationService;

        public LoginViewModel()
        {
            _navigationService = NavigationService.Instance;
        }

        [RelayCommand]
        public async Task LoginClick()
        {
            //Username = "user@user.com";
            //Password = "password";
            if (Username != null && Password != null)
            {
              
                await NetworkService.Instance.Login(Username, Password);

                if (NetworkService.Instance.TokenMem != null)
                {
                    var queryParameters = new Dictionary<string, object>
                    {
                        { "Username", Username }
                    };
                    _navigationService.Navigate<Pages.Index>(queryParameters);
                }
                else
                {
                    _navigationService.Navigate<Pages.Index>(new Dictionary<string, object> {});
                }
            }

        }

    }
}
