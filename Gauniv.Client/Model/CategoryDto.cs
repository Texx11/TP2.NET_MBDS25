namespace Gauniv.Client.Model
{
    public class CategoryDto
    {
        public int Id { get; set; }
        public String? Name { get; set; }
        public List<String> Games { get; set; } = new List<String>();
    }
}