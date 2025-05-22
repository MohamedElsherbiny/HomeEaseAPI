using HomeEase.Domain.Entities;
using HomeEase.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace HomeEase.Infrastructure.Data;

public class AdminDataSeeder
{
    public static async Task SeedAdminUserAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<AdminDataSeeder>>();

        try
        {
            var existingAdmin = await userManager.FindByEmailAsync("admin1@homeeaseapp.com");
            if (existingAdmin == null)
            {
                logger.LogInformation("No admin user found. Creating default admin user.");

                var adminUser = new User
                {
                    UserName = "admin1@homeeaseapp.com",
                    Email = "admin1@homeeaseapp.com",
                    FirstName = "System",
                    LastName = "Administrator",
                    Role = UserRole.Admin,
                    PhoneNumber = "+1234567890",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    ProfileImageUrl = "",
                    RefreshToken = ""
                };

                var result = await userManager.CreateAsync(adminUser, "Admin@123");

                if (result.Succeeded)
                {
                    // Add role as Identity role if needed
                    //await userManager.AddToRoleAsync(adminUser, UserRole.Admin.ToString());

                    logger.LogInformation("Default admin user created successfully.");
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        logger.LogError($"Error creating admin user: {error.Description}");
                    }
                }
            }
            else
            {
                logger.LogInformation("Admin user already exists. Skipping seed.");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding the admin user.");
            throw;
        }
    }
}
