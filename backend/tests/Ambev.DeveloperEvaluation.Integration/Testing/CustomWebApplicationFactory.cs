using System.Linq;
using Ambev.DeveloperEvaluation.ORM;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Ambev.DeveloperEvaluation.Integration.Testing;

public class CustomWebApplicationFactory : WebApplicationFactory<Ambev.DeveloperEvaluation.WebApi.Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<DefaultContext>));
            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            services.AddDbContext<DefaultContext>(options =>
            {
                options.UseInMemoryDatabase("IntegrationTestsDB");
            });

            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<DefaultContext>();
            db.Database.EnsureDeleted();
            db.Database.EnsureCreated();

            // Seed admin user for auth integration
            try
            {
                var users = scope.ServiceProvider.GetRequiredService<Ambev.DeveloperEvaluation.Domain.Repositories.IUserRepository>();
                var hasher = scope.ServiceProvider.GetRequiredService<Ambev.DeveloperEvaluation.Common.Security.IPasswordHasher>();
                var existing = users.GetByEmailAsync("admin@local", default).GetAwaiter().GetResult();
                if (existing is null)
                {
                    var admin = new Ambev.DeveloperEvaluation.Domain.Entities.User
                    {
                        Username = "Admin",
                        Email = "admin@local",
                        Phone = "(00) 00000-0000",
                        Password = hasher.HashPassword("Admin@123"),
                        Role = Ambev.DeveloperEvaluation.Domain.Enums.UserRole.Admin,
                        Status = Ambev.DeveloperEvaluation.Domain.Enums.UserStatus.Active,
                        CreatedAt = DateTime.UtcNow
                    };
                    users.CreateAsync(admin, default).GetAwaiter().GetResult();
                }
            }
            catch { /* ignore seed failures in tests */ }
        });
    }
}
