namespace BlazorFactoryDemo.Service;

using BlazorFactoryDemo.Data;

using global::BlazorFactoryDemo.Model;

using Microsoft.EntityFrameworkCore;

using System;

public sealed class ProductService(IDbContextFactory<CustomAppDbContext> dbFactory)
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="ct"></param>
    /// <returns></returns>
    public async Task<List<Product>> GetAllAsync(CancellationToken ct = default) => await dbFactory.RunWithDbContext(db => db.Products
        .OrderByDescending(x => x.CreatedAtUtc)
        .ToListAsync(ct), ct);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="name"></param>
    /// <param name="price"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public async Task<Guid> AddAsync(string name, decimal price, CancellationToken ct = default) => await dbFactory.RunWithDbContext(async db =>
    {
        var entity = new Product
        {
            Name = name,
            Price = price
        };
        db.Products.Add(entity);
        await db.SaveChangesAsync(ct);
        return entity.Id;
    }, ct);
    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public async Task DeleteAsync(Guid id, CancellationToken ct = default) => await dbFactory.RunWithDbContext(async db =>
    {
        var entity = await db.Products.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is not null)
        {
            db.Products.Remove(entity);
            await db.SaveChangesAsync(ct);
        }
    }, ct);
}
