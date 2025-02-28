using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Gauniv.Client.Model
{
    public class Game
    {
        public int Id { get; set; }
        public String? Name { get; set; }
        public String? Description { get; set; }

        public float Price { get; set; }

        public ICollection<Category> Categories { get; set; } = new List<Category>();
    }
}
