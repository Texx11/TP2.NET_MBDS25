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
    public class EditGameController(ILogger<GameController> logger, ApplicationDbContext applicationDbContext, UserManager<User> userManager) : Controller
    {
        private readonly ILogger<GameController> _logger = logger;
        private readonly ApplicationDbContext applicationDbContext = applicationDbContext;
        private readonly UserManager<User> userManager = userManager;

        private readonly ApplicationDbContext _context = applicationDbContext;

        [HttpGet]
        public IActionResult EditGame(int id)
        {
            var game = _context.Games.Find(id);
            if (game == null)
            {
                return NotFound();
            }

            var model = new EditViewModel
            {
                Id = game.Id,
                Name = game.Name,
                Description = game.Description,
                Price = game.Price,
            };

            return View(model);
        }

        [HttpPost]
        public IActionResult Edit(EditViewModel model, IFormFile? payloadFile)
        {
            if (ModelState.IsValid)
            {
                // Récupérer le jeu existant dans la base de données
                var game = _context.Games.Find(model.Id);
                if (game == null)
                {
                    return NotFound(); // Gérer le cas où le jeu n'existe pas
                }
                // Mettre à jour les champs
                game.Name = model.Name;
                game.Description = model.Description;
                game.Price = model.Price;

                // Gestion du fichier s'il est mis à jour
                if (payloadFile != null)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        payloadFile.CopyTo(memoryStream);
                        game.Payload = memoryStream.ToArray();
                    }
                }
                // Sauvegarder les changements
                _context.SaveChanges();

                // Rediriger vers l'index
                return RedirectToAction("Index", "Home");
            }

            return View(model);
        }
    }
}
