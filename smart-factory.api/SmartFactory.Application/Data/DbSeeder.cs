using Microsoft.EntityFrameworkCore;
using SmartFactory.Application.Entities;

namespace SmartFactory.Application.Data;

public class DbSeeder
{
    private readonly ApplicationDbContext _context;

    public DbSeeder(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task SeedAsync()
    {
        try
        {
            // Seed Admin User
            await SeedAdminUserAsync();

            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            // Log error nếu cần
            Console.WriteLine($"Error seeding database: {ex.Message}");
            throw;
        }
    }

    private async Task SeedAdminUserAsync()
    {
        // Kiểm tra xem đã có admin user chưa
        var adminExists = await _context.Users.AnyAsync(u => u.Email == "admin@smartfactory.com");

        if (!adminExists)
        {
            var adminUser = new User
            {
                Id = Guid.NewGuid(),
                Email = "admin@smartfactory.com",
                FullName = "Administrator",
                // Password: Admin@123 (đã hash)
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                PhoneNumber = "0123456789",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await _context.Users.AddAsync(adminUser);
            Console.WriteLine("✓ Admin user created successfully");
            Console.WriteLine("  Email: admin@smartfactory.com");
            Console.WriteLine("  Password: Admin@123");
        }
        else
        {
            Console.WriteLine("✓ Admin user already exists");
        }
    }
}

