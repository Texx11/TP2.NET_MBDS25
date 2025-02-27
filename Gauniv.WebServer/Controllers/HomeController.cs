using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using CommunityToolkit.HighPerformance;
using Gauniv.WebServer.Data;
using Gauniv.WebServer.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuGet.Packaging;
using X.PagedList.Extensions;

namespace Gauniv.WebServer.Controllers
{
    public class HomeController(ILogger<HomeController> logger, ApplicationDbContext applicationDbContext, UserManager<User> userManager) : Controller
    {
        private readonly ILogger<HomeController> _logger = logger;
        private readonly ApplicationDbContext applicationDbContext = applicationDbContext;
        private readonly UserManager<User> userManager = userManager;

        private readonly ApplicationDbContext _context = applicationDbContext;

        public async Task<IActionResult> Index(
    [FromQuery] string? searchName,      // nom du jeu
    [FromQuery] float? minPrice,
    [FromQuery] float? maxPrice,
    [FromQuery] List<int>? selectedCategories,
    [FromQuery] bool? possessed,
    [FromQuery] int? minSize,
    [FromQuery] int? maxSize
)
        {
            // 1) Construire la requête de base
            var gamesQuery = _context.Games
                .Include(g => g.Categories)
                .OrderBy(g => g.Id)
                .AsQueryable();

            // 2) Appliquer les filtres
            if (!string.IsNullOrWhiteSpace(searchName))
            {
                gamesQuery = gamesQuery.Where(g => g.Name != null && g.Name.Contains(searchName));
            }

            if (minPrice.HasValue)
            {
                gamesQuery = gamesQuery.Where(g => g.Price >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                gamesQuery = gamesQuery.Where(g => g.Price <= maxPrice.Value);
            }

            if (selectedCategories != null && selectedCategories.Count > 0)
            {
                // Jeux possédant *au moins* une des catégories filtrées
                gamesQuery = gamesQuery.Where(g => g.Categories.Any(c => selectedCategories.Contains(c.Id)));
            }

            // Filtrer par possédé / non possédé
            // (besoin de l'utilisateur connecté et de son "OwnedGames")
            List<int> ownedIds = new();
            if (User.Identity?.IsAuthenticated == true)
            {
                var currentUser = await userManager.GetUserAsync(User);
                if (currentUser != null)
                {
                    // Charger la liste des jeux achetés
                    var userInDb = _context.Users
                        .Where(u => u.Id == currentUser.Id)
                        .Include(u => u.OwnedGames)
                        .FirstOrDefault();

                    if (userInDb != null)
                    {
                        ownedIds = userInDb.OwnedGames.Select(g => g.Id).ToList();

                        if (possessed.HasValue)
                        {
                            if (possessed.Value)
                            {
                                // On veut seulement les jeux possédés
                                gamesQuery = gamesQuery.Where(g => ownedIds.Contains(g.Id));
                            }
                            else
                            {
                                // On veut seulement les jeux NON possédés
                                gamesQuery = gamesQuery.Where(g => !ownedIds.Contains(g.Id));
                            }
                        }
                    }
                }
            }
            else
            {
                if (possessed == true)
                {
                    gamesQuery = gamesQuery.Where(g => false); // Personne n'est connecté, donc aucun jeu possédé
                }
            }

            // Filtre par taille du payload en Mo
            if (minSize.HasValue)
            {
                gamesQuery = gamesQuery.Where(g => g.Payload != null && (g.Payload.Length / (1024.0 * 1024.0)) >= minSize.Value);
            }
            if (maxSize.HasValue)
            {
                gamesQuery = gamesQuery.Where(g => g.Payload != null && (g.Payload.Length / (1024.0 * 1024.0)) <= maxSize.Value);
            }

            // 3) Exécuter la requête
            var games = gamesQuery.ToList();

            // 4) Préparer le modèle pour la vue
            var viewModel = new IndexViewModel
            {
                Games = games,
                OwnedGameIds = ownedIds,
                SearchName = searchName,
                MinPrice = minPrice,
                MaxPrice = maxPrice,
                SelectedCategories = selectedCategories ?? new List<int>(),
                Possessed = possessed,
                MinSize = minSize,
                MaxSize = maxSize
            };

            return View(viewModel);
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            // Trouver le jeu à supprimer
            var game = _context.Games.Find(id);
            if (game == null)
            {
                return NotFound(); // Si le jeu n'existe pas
            }

            // Supprimer le jeu de la base de données
            _context.Games.Remove(game);
            _context.SaveChanges();

            // Rediriger vers l'index après la suppression
            return RedirectToAction("Index");
        }

        [Authorize] 
        [HttpGet]
        public IActionResult OnlineUsers()
        {
            return View();
        }
    }
}
