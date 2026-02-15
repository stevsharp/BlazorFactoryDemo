namespace BlazorFactoryDemo.Data;

using BlazorFactoryDemo.Model;

using Microsoft.EntityFrameworkCore;

public static class DbSeeder
{
    public static Task SeedAsync(
        IDbContextFactory<CustomAppDbContext> factory,
        CancellationToken ct = default)
        => factory.RunWithDbContext(async db =>
        {
            await db.Database.MigrateAsync(ct);

            if (await db.Products.AnyAsync(ct))
                return;

            db.Products.AddRange(
                new Product
                {
                    Id = Guid.NewGuid(),
                    Name = "Demo Shampoo",
                    Price = 12.90m,
                    CreatedAtUtc = DateTime.UtcNow
                },
                new Product
                {
                    Id = Guid.NewGuid(),
                    Name = "Hair Serum",
                    Price = 24.50m,
                    CreatedAtUtc = DateTime.UtcNow.AddMinutes(-10)
                },
                new Product
                {
                    Id = Guid.NewGuid(),
                    Name = "Conditioner",
                    Price = 9.99m,
                    CreatedAtUtc = DateTime.UtcNow.AddHours(-1)
                }
            );

            await db.SaveChangesAsync(ct);
        }, ct);
}
