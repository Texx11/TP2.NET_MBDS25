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
    public class GameController(ILogger<GameController> logger, ApplicationDbContext applicationDbContext, UserManager<User> userManager) : Controller
    {
        private readonly ILogger<GameController> _logger = logger;
        private readonly ApplicationDbContext applicationDbContext = applicationDbContext;
        private readonly UserManager<User> userManager = userManager;

        private readonly ApplicationDbContext _context = applicationDbContext;

        [HttpGet]
        public IActionResult CreateGame()
        {
            var model = new GameViewModel
            {
                Categories = _context.Categories.ToList(),

                AvailableCategories = _context.Categories
                        .Select(c => new SelectListItem
                        {
                            Value = c.Id.ToString(),
                            Text = c.Name
                        }).ToList()
            };

            return View(model);
        }


        [HttpPost]
        public IActionResult Create(GameViewModel model, IFormFile? payloadFile)
        {
            if (ModelState.IsValid)
            {
                byte[]? fileBytes = null;
                if (payloadFile != null)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        payloadFile.CopyTo(memoryStream);
                        fileBytes = memoryStream.ToArray();
                    }
                }

                // Create the new game object
                var game = new Game
                {
                    Name = model.Name,
                    Description = model.Description,
                    Price = model.Price,
                    Payload = fileBytes
                };

                // Initialize the list of categories for the new game
                game.Categories = new List<Category>();

                // Add selected categories to the game
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

                // Save the new game to the database
                _context.Games.Add(game);
                _context.SaveChanges();

                return RedirectToAction("Index", "Home");
            }

            // If the model state is invalid, re-populate the AvailableCategories list and return the view with the model
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
