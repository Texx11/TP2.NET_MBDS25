using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;

namespace Gauniv.Client.Model
{
    public partial class GameItem : ObservableObject
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public float Price { get; set; }
        public List<string> Categories { get; set; } = new List<string>();

        public string CategoriesString => string.Join(", ", Categories);

        [ObservableProperty]
        private bool isDownloaded; 
    }
}
