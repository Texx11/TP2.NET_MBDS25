namespace Gauniv.Client.Model
{
    public class GameItem
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public float Price { get; set; }
        public List<string> Categories { get; set; } = new List<string>();

        // Propriété calculée pour l'affichage des catégories
        public string CategoriesString => string.Join(", ", Categories);
    }
}
