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

        public IActionResult Index()
        {
            // Retrieve the list of games from the database
            List<Game> games = _context.Games.OrderBy(g => g.Id).ToList();
            //TODO: Order the list by the id

            return View(new IndexViewModel { Games = games });
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            // Trouver le jeu � supprimer
            var game = _context.Games.Find(id);
            if (game == null)
            {
                return NotFound(); // Si le jeu n'existe pas
            }

            // Supprimer le jeu de la base de donn�es
            _context.Games.Remove(game);
            _context.SaveChanges();

            // Rediriger vers l'index apr�s la suppression
            return RedirectToAction("Index");
        }
    }
}
