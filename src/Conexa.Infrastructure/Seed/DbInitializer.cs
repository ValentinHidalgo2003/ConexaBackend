using Conexa.Application.Interfaces;
using Conexa.Domain.Constants;
using Conexa.Domain.Entities;
using Conexa.Infrastructure.Configuration;
using Conexa.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Conexa.Infrastructure.Seed;

public static class DbInitializer
{
    public static async Task InitializeAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var scopedServices = scope.ServiceProvider;
        var logger = scopedServices.GetRequiredService<ILoggerFactory>().CreateLogger("DbInitializer");
        var seedSettings = scopedServices.GetRequiredService<IOptions<SeedSettings>>().Value;

        if (!seedSettings.Enabled)
        {
            return;
        }

        var dbContext = scopedServices.GetRequiredService<AppDbContext>();
        await dbContext.Database.MigrateAsync();

        var roleManager = scopedServices.GetRequiredService<RoleManager<IdentityRole>>();
        await EnsureRoleAsync(roleManager, Roles.User);
        await EnsureRoleAsync(roleManager, Roles.Admin);

        var userManager = scopedServices.GetRequiredService<UserManager<ApplicationUser>>();
        var adminUser = await userManager.FindByEmailAsync(seedSettings.AdminEmail);
        if (adminUser is null)
        {
            adminUser = new ApplicationUser
            {
                UserName = seedSettings.AdminEmail,
                Email = seedSettings.AdminEmail,
                FullName = seedSettings.AdminFullName,
                EmailConfirmed = true
            };

            var createResult = await userManager.CreateAsync(adminUser, seedSettings.AdminPassword);
            if (!createResult.Succeeded)
            {
                var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
                logger.LogError("Failed to create admin user: {Errors}", errors);
                return;
            }

            await userManager.AddToRoleAsync(adminUser, Roles.Admin);
            logger.LogInformation("Admin user seeded with email {Email}", seedSettings.AdminEmail);
        }

        if (seedSettings.SyncSwapiOnStartup)
        {
            var movieRepository = scopedServices.GetRequiredService<IMovieRepository>();
            if (await movieRepository.CountAsync() == 0)
            {
                var syncService = scopedServices.GetRequiredService<ISwapiSyncService>();
                var result = await syncService.SyncFilmsAsync();
                logger.LogInformation(
                    "Initial SWAPI sync completed. Created: {Created}, Updated: {Updated}, Total: {Total}",
                    result.Created,
                    result.Updated,
                    result.Total);
            }
        }
    }

    private static async Task EnsureRoleAsync(RoleManager<IdentityRole> roleManager, string roleName)
    {
        if (!await roleManager.RoleExistsAsync(roleName))
        {
            await roleManager.CreateAsync(new IdentityRole(roleName));
        }
    }
}
