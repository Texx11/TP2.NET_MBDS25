using Gauniv.Client.Model;

namespace Gauniv.Client.Pages;

public partial class GameDetails : ContentPage
{
	public GameDetails()
	{
		InitializeComponent();
	}

    public List<Game> getGames()
    {
        return new List<Game>
        {
            new Game { Id = 1, Name = "Game 1", Description = "Description 1", Price = 10.0f },
            new Game { Id = 2, Name = "Game 2", Description = "Description 2", Price = 20.0f },
            new Game { Id = 3, Name = "Game 3", Description = "Description 3", Price = 30.0f },
            new Game { Id = 4, Name = "Game 4", Description = "Description 4", Price = 40.0f },
            new Game { Id = 5, Name = "Game 5", Description = "Description 5", Price = 50.0f },
            new Game { Id = 6, Name = "Game 6", Description = "Description 6", Price = 60.0f },
            new Game { Id = 7, Name = "Game 7", Description = "Description 7", Price = 70.0f },
            new Game { Id = 8, Name = "Game 8", Description = "Description 8", Price = 80.0f },
        };
    }

    public List<Category> GetCategories()
    {
        return new List<Category> {
            new Category { Id = 1, Name = "Category 1" },
            new Category { Id = 2, Name = "Category 2" },
            new Category { Id = 3, Name = "Category 3" },
            new Category { Id = 4, Name = "Category 4" },
            new Category { Id = 5, Name = "Category 5" },
            new Category { Id = 6, Name = "Category 6" },
            new Category { Id = 7, Name = "Category 7" },
            new Category { Id = 8, Name = "Category 8" },
        };
    }
}