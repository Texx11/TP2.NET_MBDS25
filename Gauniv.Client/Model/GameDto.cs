
namespace Gauniv.Client.Model
{
    public class GameDto
    {
        public int Id { get; set; }
        public String? Name { get; set; }
        public String? Description { get; set; }
        public float Price { get; set; }
        public List<String> Categories { get; set; } = new List<String>();
    }
}