using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Gauniv.WebServer.Data
{
    public class User : IdentityUser
    {
        // Nouveau : Prénom et Nom
        public string? FirstName { get; set; }
        public string? LastName { get; set; }

        // Nouveau : Liste des jeux achetés (relation Many-to-Many)
        public ICollection<Game> OwnedGames { get; set; } = new List<Game>();
    }
}
