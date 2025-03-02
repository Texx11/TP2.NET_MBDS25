using Gauniv.WebServer.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Gauniv.WebServer.Services
{
    public class SetupService : IHostedService
    {
        private readonly IServiceProvider serviceProvider;

        public SetupService(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();

            // Appliquer d'éventuelles migrations en attente
            if (context.Database.GetPendingMigrations().Any())
            {
                context.Database.Migrate();
            }

            // 1) Créer le rôle Admin s’il n’existe pas
            if (!roleManager.RoleExistsAsync("Admin").Result)
            {
                var r = new IdentityRole("Admin");
                roleManager.CreateAsync(r).Wait();
            }

            // 2) Création d’un user Admin de test si besoin
            const string adminEmail = "admin@oui.com";
            var adminUser = userManager.FindByEmailAsync(adminEmail).Result;
            if (adminUser == null)
            {
                adminUser = new User
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FirstName = "AdminFirstName",
                    LastName = "AdminLastName",
                };
                // Mot de passe de test => à changer en prod
                userManager.CreateAsync(adminUser, "admin").Wait();
            }

            // 3) On l’ajoute au rôle Admin s’il n’y est pas déjà
            if (!userManager.IsInRoleAsync(adminUser, "Admin").Result)
            {
                userManager.AddToRoleAsync(adminUser, "Admin").Wait();
            }

            // 4) Création de quelques utilisateurs de test (simulés)
            string[] sampleUserEmails = { "user1@domain.com", "user2@domain.com", "user3@domain.com" };
            foreach (var email in sampleUserEmails)
            {
                var user = userManager.FindByEmailAsync(email).Result;
                if (user == null)
                {
                    user = new User
                    {
                        UserName = email,
                        Email = email,
                        FirstName = "UserFirstName",  
                        LastName = "UserLastName"
                    };
                    // Mot de passe commun pour les utilisateurs de test
                    userManager.CreateAsync(user, "user").Wait();
                }
            }
            context.SaveChanges();

            if (!context.Games.Any())
            {
                Console.WriteLine("🎮 Base de données vide, ajout des jeux et catégories...");

                // 4) Création des catégories
                var categories = new List<Category>
                {
                    new Category { Name = "Action" },
                    new Category { Name = "Aventure" },
                    new Category { Name = "Stratégie" },
                    new Category { Name = "RPG" },
                    new Category { Name = "Guerre" }
                };

                context.Categories.AddRange(categories);
                context.SaveChanges();

                // 5) Ajout des jeux de test avec un fichier binaire
                string sampleFilePath = "C:\\Windows\\notepad.exe"; // Remplacer par un fichier de votre choix

                if (!File.Exists(sampleFilePath))
                {
                    Console.WriteLine("Fichier non trouvé : " + sampleFilePath);
                }
                else
                {
                    byte[] filePayload = File.ReadAllBytes(sampleFilePath);

                    var games = new List<Game>
                    {
                        new Game { Name = "Doom", Description = "FPS classique", Price = 19.99f, Payload = filePayload, Categories = new List<Category> { context.Categories.First(c => c.Name == "Action") } },
                        new Game { Name = "Zelda", Description = "Aventure épique", Price = 49.99f, Payload = filePayload, Categories = new List<Category> { context.Categories.First(c => c.Name == "Aventure") } },
                        new Game { Name = "League of Legends", Description = "Jeu 5v5 avec Faker", Price = 9.99f, Payload = filePayload, Categories = new List<Category> { context.Categories.First(c => c.Name == "RPG") } },
                        new Game { Name = "Counter Strike", Description = "Jeu de guerre en 5v5 avec Ziwoo", Price = 14.99f, Payload = filePayload, Categories = new List<Category> { context.Categories.First(c => c.Name == "Guerre") } },
                        new Game { Name = "World of Warcraft", Description = "Univers medieval Fantastique avec des Orcs et des Humains", Price = 24.99f, Payload = filePayload, Categories = new List<Category> { context.Categories.First(c => c.Name == "RPG") } },
                        new Game { Name = "DOFUS" , Description = "Meilleur jeu du monde, DOFUS > WoW, by Ankama Games", Price = 4.99f, Payload = filePayload, Categories = new List<Category> { context.Categories.First(c => c.Name == "RPG") } },
                        new Game { Name = "Civilization", Description = "Jeu de stratégie ou vous devez gérer une civilisation (trop bien)", Price = 29.99f, Payload = filePayload, Categories = new List<Category> { context.Categories.First(c => c.Name == "Stratégie") } },
                        new Game { Name = "Age of Empire", Description = "Jeu de stratégie ou vous devez gérer une civilisation", Price = 79.99f, Payload = filePayload, Categories = new List<Category> { context.Categories.First(c => c.Name == "Stratégie"), context.Categories.First(c => c.Name == "Action") } },
                        new Game { Name = "Call of Duty", Description = "Jeu de guerre qui était encore bien en 2015", Price = 8.88f, Payload = filePayload, Categories = new List<Category> { context.Categories.First(c => c.Name == "Guerre"), context.Categories.First(c => c.Name == "Action") } },
                        new Game { Name = "Tetris", Description = "Jeu avec des cube et il faut les encastrer",  Payload = filePayload, Categories = new List<Category> { context.Categories.First(c => c.Name == "Stratégie") } },
                        new Game { Name = "Mario", Description = "Jeu de plateforme avec un plombier qui saute", Price = 19.99f, Payload = filePayload, Categories = new List<Category> { context.Categories.First(c => c.Name == "Aventure") } },
                        new Game { Name = "Fortnite", Description = "Jeu de Battle Royale avec des gosses de 12 ans qui te fume", Price = 0f, Payload = filePayload, Categories = new List<Category> { context.Categories.First(c => c.Name == "Action") } }
                    };

                    context.Games.AddRange(games);
                    context.SaveChanges();
                }
            }

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
