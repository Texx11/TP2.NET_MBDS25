using AutoMapper;
using Gauniv.WebServer.Data;
using Gauniv.WebServer.Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using System.Text;
using CommunityToolkit.HighPerformance.Memory;
using CommunityToolkit.HighPerformance;
using Microsoft.AspNetCore.Authorization;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Gauniv.WebServer.Api

    /**
     * Microsoft.Hosting.Lifetime: Information: Now listening on: https://localhost:7209
     * Microsoft.Hosting.Lifetime: Information: Now listening on: http://localhost:5231
     */
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly ApplicationDbContext appDbContext;
        private readonly IMapper mapper;
        private readonly UserManager<User> userManager;

        public CategoryController(ApplicationDbContext appDbContext, IMapper mapper, UserManager<User> userManager)
        {
            this.appDbContext = appDbContext;
            this.mapper = mapper;
            this.userManager = userManager;
        }

        /**
         * Liste des categories
         * --------------------------------------------
         * Test : http://localhost:5231/category
         */
        [HttpGet("/category")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetCategories(
            [FromQuery] int offset = 0,
            [FromQuery] int limit = 10)
        {

            if (limit <= 0 || limit > 50)
            {
                return BadRequest("Limit must be between 1 and 50.");
            }
            if (offset < 0)
            {
                return BadRequest("Offset must be non-negative.");
            }

            var query = appDbContext.Categories.AsQueryable();

            var categories = await query
                .Skip(offset)
                .Take(limit)
                .Select(c => new CategoryDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Games = c.Games.Select(c => c.Name!).ToList()
                })
                .ToListAsync();

            return Ok(categories);
        }
    }
}
