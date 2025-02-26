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
    public class CategoryController(ILogger<CategoryController> logger, ApplicationDbContext applicationDbContext, UserManager<User> userManager) : Controller
    {
        private readonly ILogger<CategoryController> _logger = logger;
        private readonly ApplicationDbContext applicationDbContext = applicationDbContext;
        private readonly UserManager<User> userManager = userManager;

        private readonly ApplicationDbContext _context = applicationDbContext;

        [HttpGet]
        public IActionResult CreateCategory()
        {
            return View();
        }

        [HttpGet]
        public IActionResult ListCategory()
        {
            List<Category> categories = _context.Categories.OrderBy(g => g.Id).ToList();

            return View(categories);
        }

        [HttpPost]
        public IActionResult Create(CreateCategoryViewModel model, IFormFile? payloadFile)
        {
            if (ModelState.IsValid)
            {

                Category category = new Category
                {
                    Name = model.Name,
                };
                _context.Add(category);
                _context.SaveChanges();

                return RedirectToAction("Index", "Home");
            }

            return View(model);
        }
    }
}
