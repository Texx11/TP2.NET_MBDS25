using Gauniv.WebServer.Data;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Runtime.InteropServices;

namespace Gauniv.WebServer.Models
{
    public class CreateViewModel()
    {
        public string Name { get; set; }
        public float Price { get; set; }
        public IFormFile Content { get; set; }
        public int[] Categories { get; set; }
    }
    public class GameViewModel()
    {
        public int Id { get; set; }
        public string? Name { get; set; }

        public string? Description { get; set; }
        public float Price { get; set; }
        public byte[]? Payload { get; set; }

        public List<Category> Categories { get; set; } = new List<Category>();
        public List<SelectListItem> AvailableCategories { get; set; } = new List<SelectListItem>();
        public List<int> SelectedCategoryIds { get; set; } = new List<int>();

    }

    /**
     * This class is used to display the list of games
     */
    public class IndexViewModel()
    {
        public required List<Game> Games { get; set; }
        public List<int> OwnedGameIds { get; set; } = new();
    }

    /**
     * This class is used to create a game
     */
    public class CreateGameViewModel()
    {
        public String? Name { get; set; }
        public String? Description { get; set; }
        public float Price { get; set; }
        public String[]? Categories { get; set; }
        public byte[]? Payload { get; set; }
    }

    /**
     * View Model pour afficher des categories
     */
    public class CategoryViewModel()
    {
        public int Id { get; set; }
        public String? Name { get; set; }

        public required List<Category> Categories {get; set;}
    }


}
