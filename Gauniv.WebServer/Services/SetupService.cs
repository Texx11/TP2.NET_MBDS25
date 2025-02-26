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
                userManager.CreateAsync(adminUser, "adminpassword123").Wait();
            }

            // 3) On l’ajoute au rôle Admin s’il n’y est pas déjà
            if (!userManager.IsInRoleAsync(adminUser, "Admin").Result)
            {
                userManager.AddToRoleAsync(adminUser, "Admin").Wait();
            }

            context.SaveChanges();

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
