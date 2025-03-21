﻿using AutoMapper;
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
    [Route("api/game/[action]")]
    [ApiController]
    public class GamesController : ControllerBase
    {
        private readonly ApplicationDbContext appDbContext;
        private readonly IMapper mapper;
        private readonly UserManager<User> userManager;

        public GamesController(ApplicationDbContext appDbContext, IMapper mapper, UserManager<User> userManager)
        {
            this.appDbContext = appDbContext;
            this.mapper = mapper;
            this.userManager = userManager;
        }

        /**
         * Liste des jeux disponibles pour tout le monde
         * --------------------------------------------
         * Test : http://localhost:5231/api/game
         */
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ActionName("")]
        public async Task<ActionResult<IEnumerable<GameDto>>> GetGames(
            [FromQuery] int offset = 0,
            [FromQuery] int limit = 10,
            [FromQuery] string? category = null)
        {
            if (offset < 0)
            {
                return BadRequest("Offset must be non-negative.");
            }

            if (limit <= 0 || limit > 50)
            {
                return BadRequest("Limit must be between 1 and 50.");
            }

            var query = appDbContext.Games.Include(g => g.Categories).AsQueryable();

            if (!string.IsNullOrEmpty(category))
            {
                var categoryIds = category.Split(',')
                                            .Select(int.Parse)
                                            .ToList();

                query = query.Where(g => g.Categories.Any(c => categoryIds.Contains(c.Id)));
            }

            var games = await query
                        .Skip(offset)
                        .Take(limit)
                        .Select(g => new GameDto
                        {
                            Id = g.Id,
                            Name = g.Name,
                            Description = g.Description,
                            Price = g.Price,
                            Categories = g.Categories.Select(c => c.Name!).ToList()
                        })
                        .ToListAsync();

            return games;
        }

        /**
         * Liste des jeux disponibles pour tout le monde
         * --------------------------------------------
         * Test : http://localhost:5231/api/game/mygames
         */

        [HttpGet]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ActionName("mygames")]
        public async Task<ActionResult<IEnumerable<GameDto>>> GetUserGames(
            [FromQuery] int offset = 0,
            [FromQuery] int limit = 10,
            [FromQuery] string? category = null)
        {
            if (offset < 0)
            {
                return BadRequest("Offset must be non-negative.");
            }

            if (limit <= 0 || limit > 50)
            {
                return BadRequest("Limit must be between 1 and 50.");
            }

            var user = await userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            var query = appDbContext.Games
                .Where(g => g.Owners.Contains(user))
                .Include(g => g.Categories)
                .AsNoTracking();

            if (!string.IsNullOrEmpty(category))
            {
                var categoryIds = category.Split(',')
                                            .Select(int.Parse)
                                            .ToList();

                query = query.Where(g => g.Categories.Any(c => categoryIds.Contains(c.Id)));
            }

            var userGames = await query
                .Skip(offset)
                .Take(limit)
                .Select(g => new GameDto
                {
                    Id = g.Id,
                    Name = g.Name,
                    Description = g.Description,
                    Price = g.Price,
                    Categories = g.Categories.Select(c => c.Name!).ToList()
                })
                .ToListAsync();

            return userGames;
        }

        /**
         * Recupère le binaire d'un jeu avec son Id
         * Test : http://localhost:5231/api/game/1/download
         */
        [HttpGet("{gameId}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ActionName("download")]
        public async Task<ActionResult> DownloadGame(int gameId)
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            // TODO : Download a game from API
            var game = await appDbContext.Games.FindAsync(gameId);
            if (game == null)
            {
                return BadRequest("Game not found.");
            }
            string fileName = string.IsNullOrWhiteSpace(game.Name) ? "game.bin" : game.Name;
            if (game.Payload != null)
            {
                return File(game.Payload, "application/octet-stream", fileName);
            }
            //Return empty binary file of txt application
            return File(Array.Empty<byte>(), "application/octet-stream", fileName);
        }

        /**
         * Ajoute un jeu a la liste des jeux de l'utilisateur
         * --------------------------------------------
         * Test : http://localhost:5231/game/buy/1
         */

        [HttpPost("{gameId}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ActionName("buy")]
        public async Task<ActionResult> BuyGame(int gameId)
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }
            var game = await appDbContext.Games
                .Include(g => g.Owners)
                .FirstOrDefaultAsync(g => g.Id == gameId);
            if (game == null)
            {
                return BadRequest("Game not found.");
            }
            game.Owners.Add(user);
            await appDbContext.SaveChangesAsync();
            return Ok();
        }
    }
}
