using BlazorFactoryDemo.Model;

using Microsoft.EntityFrameworkCore;

namespace BlazorFactoryDemo.Data;

public sealed class CustomAppDbContext(DbContextOptions<CustomAppDbContext> options) : DbContext(options)
{
    public DbSet<Product> Products => Set<Product>();   


}
