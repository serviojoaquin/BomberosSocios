using BaseApp.Domain.Constants;
using BaseApp.Domain.Entities;
using BaseApp.Domain.Enums;
using BaseApp.Domain.ValueObjects;
using BaseApp.Infrastructure.Identity;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BaseApp.Infrastructure.Data;

public static class InitialiserExtensions
{
    public static async Task InitialiseDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        var initialiser = scope.ServiceProvider.GetRequiredService<ApplicationDbContextInitialiser>();

        await initialiser.InitialiseAsync();
        await initialiser.SeedAsync();
    }
}

public class ApplicationDbContextInitialiser
{
    private readonly ILogger<ApplicationDbContextInitialiser> _logger;
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;

    public ApplicationDbContextInitialiser(ILogger<ApplicationDbContextInitialiser> logger, ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager)
    {
        _logger = logger;
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task InitialiseAsync()
    {
        try
        {
            await _context.Database.MigrateAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while initialising the database.");
            throw;
        }
    }

    public async Task SeedAsync()
    {
        try
        {
            await TrySeedAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
    }

    public async Task TrySeedAsync()
    {
        await EnsureRoleAsync(Roles.Administrator, Permissions.All);
        await EnsureRoleAsync(Roles.Viewer, Permissions.ViewSampleItems);

        await EnsureUserAsync("administrator@localhost", "Administrator1!", Roles.Administrator);
        await EnsureUserAsync("viewer@localhost", "Viewer1!", Roles.Viewer);

        if (!_context.TodoLists.Any())
        {
            _context.TodoLists.Add(new TodoList
            {
                Title = "Tasks",
                Colour = Colour.Green,
                Items =
                {
                    new TodoItem { Title = "Make a todo list 📃" },
                    new TodoItem { Title = "Check off the first item ✅" },
                    new TodoItem { Title = "Realise you've already done two things on the list! 🤯"},
                    new TodoItem { Title = "Reward yourself with a nice, long nap 🏆" },
                }
            });

            await _context.SaveChangesAsync();
        }

        if (!_context.SampleItems.Any())
        {
            _context.SampleItems.AddRange(
                new SampleItem { Description = "Standard", Amount = 1500m },
                new SampleItem { Description = "Premium", Amount = 3000m },
                new SampleItem { Description = "Student", Amount = 800m });

            await _context.SaveChangesAsync();
        }
    }

    private async Task EnsureRoleAsync(string roleName, Permissions permissions)
    {
        var role = await _roleManager.FindByNameAsync(roleName);

        if (role is null)
        {
            await _roleManager.CreateAsync(new ApplicationRole(roleName, permissions, isSystem: roleName == Roles.Administrator));
            return;
        }

        if (role.Permissions != permissions)
        {
            role.Permissions = permissions;
            await _roleManager.UpdateAsync(role);
        }
    }

    private async Task EnsureUserAsync(string userName, string password, string roleName)
    {
        if (_userManager.Users.Any(u => u.UserName == userName))
        {
            return;
        }

        var user = new ApplicationUser { UserName = userName, Email = userName };

        await _userManager.CreateAsync(user, password);
        await _userManager.AddToRolesAsync(user, [roleName]);
    }
}
