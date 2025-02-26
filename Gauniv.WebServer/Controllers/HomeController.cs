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

        public async Task<IActionResult> Index()
        {
            var games = _context.Games.Include(g => g.Categories).OrderBy(g => g.Id).ToList();
            var viewModel = new IndexViewModel { Games = games };

            if (User.Identity?.IsAuthenticated == true)
            {
                // Récupère l'utilisateur avec ses jeux possédés
                var currentUser = await userManager.GetUserAsync(User);
                var userWithGames = _context.Users
                    .Where(u => u.Id == currentUser.Id)
                    .Include(u => u.OwnedGames)
                    .FirstOrDefault();
                if (userWithGames != null)
                {
                    viewModel.OwnedGameIds = userWithGames.OwnedGames.Select(g => g.Id).ToList();
                }
            }
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
    }
}
