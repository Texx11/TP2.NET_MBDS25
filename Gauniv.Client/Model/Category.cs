using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Gauniv.Client.Model
{
    public class Category
    {
        public int Id { get; set; }

        [Required]
        public String? Name { get; set; }

        public ICollection<Game> Games { get; set; } = new List<Game>();
    }
}
