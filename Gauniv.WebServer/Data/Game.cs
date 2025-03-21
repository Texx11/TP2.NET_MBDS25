﻿using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;

namespace Gauniv.WebServer.Data
{
    public class Game
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int Id { get; set; }
        public String? Name { get; set; }
        public String? Description { get; set; }

        [Column(TypeName = "bytea")] 
        public byte[]? Payload { get; set; }
        public float Price { get; set; }

        // Link to the categories
        public ICollection<Category> Categories { get; set; } = new List<Category>();
        public ICollection<User> Owners { get; set; } = new List<User>();
    }
}
