using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using CommunityToolkit.HighPerformance;
using Gauniv.WebServer.Data;
using Gauniv.WebServer.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NuGet.Packaging;
using X.PagedList.Extensions;

namespace Gauniv.WebServer.Controllers
{
    public class EditGameController : Controller
    {
        private readonly ILogger<GameController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public EditGameController(ILogger<GameController> logger, ApplicationDbContext applicationDbContext, UserManager<User> userManager)
        {
            _logger = logger;
            _context = applicationDbContext;
            _userManager = userManager;
        }

        // Action GET pour afficher le formulaire d'édition du jeu
        [HttpGet]
        public IActionResult EditGame(int id)
        {
            var game = _context.Games.Include(g => g.Categories).FirstOrDefault(g => g.Id == id);
            if (game == null)
            {
                return NotFound();
            }

            // Préparer le modèle de données pour le formulaire
            var model = new GameViewModel
            {
                Id = game.Id,
                Name = game.Name,
                Description = game.Description,
                Price = game.Price,
                // Charger toutes les catégories disponibles
                AvailableCategories = _context.Categories
                    .Select(c => new SelectListItem
                    {
                        Value = c.Id.ToString(),
                        Text = c.Name
                    }).ToList(),
                SelectedCategoryIds = game.Categories.Select(c => c.Id).ToList(),
                Categories = _context.Categories.ToList()
            };

            return View(model);
        }

        // Action POST pour enregistrer les modifications après soumission du formulaire
        [HttpPost]
        public IActionResult Edit(GameViewModel model, IFormFile? payloadFile)
        {
            if (ModelState.IsValid)
            {
                // Récupérer le jeu existant dans la base de données
                var game = _context.Games.Include(g => g.Categories).FirstOrDefault(g => g.Id == model.Id);
                if (game == null)
                {
                    return NotFound(); // Gérer le cas où le jeu n'existe pas
                }

                game.Name = model.Name;
                game.Description = model.Description;
                game.Price = model.Price;

                game.Categories.Clear();
                if (model.Categories != null)
                {
                    foreach (var categoryId in model.SelectedCategoryIds)
                    {
                        var category = _context.Categories.FirstOrDefault(c => c.Id == categoryId);
                        if (category != null)
                        {
                            game.Categories.Add(category);
                        }
                    }
                }

                if (payloadFile != null)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        payloadFile.CopyTo(memoryStream);
                        game.Payload = memoryStream.ToArray();
                    }
                }

                _context.SaveChanges();

                return RedirectToAction("Index", "Home");
            }

            model.AvailableCategories = _context.Categories
                                                .Select(c => new SelectListItem
                                                {
                                                    Value = c.Id.ToString(),
                                                    Text = c.Name
                                                }).ToList();

            return View(model);
        }
    }

}
