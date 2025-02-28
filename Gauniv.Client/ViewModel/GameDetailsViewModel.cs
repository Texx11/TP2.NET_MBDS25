using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Gauniv.Client.Model;
using Gauniv.Client.Services;
using Gauniv.Client.WinUI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gauniv.Client.ViewModel
{
    [QueryProperty(nameof(Game), nameof(Game))]
    public partial class GameDetailsViewModel : ObservableObject
    {
        [ObservableProperty]
        public GameDto game;
    }
}
