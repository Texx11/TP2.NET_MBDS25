using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using CommunityToolkit.HighPerformance;
using Gauniv.WebServer.Data;
using Gauniv.WebServer.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Scripting.Hosting;
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

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult CreateCategory()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult ListCategory()
        {
            List<Category> categories = _context.Categories.OrderBy(c => c.Id).ToList();
            var model = new CategoryViewModel
            {
                Categories = categories
            };
            
            foreach(var c in categories)
            {
                Debug.Print(c.Id + " : " + c.Name);
            }

            return View(model);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult EditCategory(int id)
        {
            var category = _context.Categories.FirstOrDefault(c => c.Id == id);
            if (category == null)
            {
                return NotFound();
            }
            var model = new CategoryViewModel
            {
                Id = category.Id,
                Name = category.Name,
                Categories = _context.Categories.OrderBy(c => c.Id).ToList()
            };
            return View(model);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult Delete(int id)
        {
            var category = _context.Categories.Find(id);
            if (category == null)
            {
                return NotFound();
            }

            _context.Categories.Remove(category);
            _context.SaveChanges();

            return RedirectToAction("ListCategory");
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public IActionResult Create(CategoryViewModel model, IFormFile? payloadFile)
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

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public IActionResult Edit(CategoryViewModel model)
        {
            //Edit category name
            var category = _context.Categories.FirstOrDefault(c => c.Id == model.Id);
            if (category == null)
            {
                return NotFound();
            }
            category.Name = model.Name;

            _context.SaveChanges();
            
            return RedirectToAction("ListCategory");
        }
    }
}
