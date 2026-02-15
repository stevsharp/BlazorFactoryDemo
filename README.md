````md
# Blazor Server + EF Core DbContextFactory (Tutorial Project)

A minimal Blazor Server CRUD app that demonstrates the correct way to use `IDbContextFactory<TContext>` in UI driven apps, especially Blazor Server where scoped lifetimes can be long.

This repo focuses on:
- Short lived `DbContext` instances per operation
- Clean `DbContextFactory` extension methods (`RunWithDbContext`)
- A simple `ProductService` that never keeps a context alive
- Automatic migrations + demo data seeding on startup

---

## Why DbContextFactory in Blazor Server

Blazor Server circuits can live for minutes or hours. Keeping a scoped `DbContext` for that long can lead to:
- stale tracking state
- memory growth (ChangeTracker bloat)
- concurrency problems when multiple UI events overlap
- “A second operation was started on this context…” errors

`IDbContextFactory<TContext>` fixes this by creating a fresh `DbContext` per call, then disposing it immediately.

---

## Tech stack

- .NET (Blazor Server)
- EF Core
- `IDbContextFactory<TContext>`
- Any provider (SQLite, SQL Server, PostgreSQL supported)

---

## Project structure

- `Data/`
  - `CustomAppDbContext.cs`
  - `Product.cs`
  - `DbFactoryExtensions.cs` (your `RunWithDbContext` extensions)
  - `ProductService.cs`
  - `DbSeeder.cs`
- `Components/Pages/`
  - `Products.razor` (simple UI)

---

## Getting started

### 1) Clone and run
```bash
git clone <your-repo-url>
cd <your-repo-folder>
dotnet restore
dotnet run
````

Open the app and go to:

* `/products`

### 2) Create the database tables (migrations)

Install EF tool (one time):

```bash
dotnet tool install --global dotnet-ef
```

Create migration:

```bash
dotnet ef migrations add InitialCreate
```

Apply migration:

```bash
dotnet ef database update
```

---

## Database provider setup

### SQLite (quick start)

**appsettings.json**

```json
{
  "ConnectionStrings": {
    "app": "Data Source=app.db"
  }
}
```

**Program.cs**

```csharp
builder.Services.AddDbContextFactory<CustomAppDbContext>(options =>
{
    options.UseSqlite(builder.Configuration.GetConnectionString("app"));
});
```

## The core idea 

`DbFactoryExtensions.cs` contains two helpers that enforce:

* create a context
* run an operation
* dispose the context

```csharp
public static async Task<T> RunWithDbContext<T>(
    this IDbContextFactory<CustomAppDbContext> factory,
    Func<CustomAppDbContext, Task<T>> action,
    CancellationToken cancellation)
{
    await using var db = await factory.CreateDbContextAsync(cancellation);
    return await action(db);
}

public static async Task RunWithDbContext(
    this IDbContextFactory<CustomAppDbContext> factory,
    Func<CustomAppDbContext, Task> action,
    CancellationToken ct = default)
{
    await using var db = await factory.CreateDbContextAsync(ct);
    await action(db);
}
```

---

## ProductService pattern

All database work is short lived, and no `DbContext` is stored in fields.

```csharp
public sealed class ProductService(IDbContextFactory<CustomAppDbContext> factory)
{
    public Task<List<Product>> GetAllAsync(CancellationToken ct) =>
        factory.RunWithDbContext(
            db => db.Products.AsNoTracking()
                .OrderByDescending(x => x.CreatedAtUtc)
                .ToListAsync(ct),
            ct);

    public Task<Guid> AddAsync(string name, decimal price, CancellationToken ct) =>
        factory.RunWithDbContext(async db =>
        {
            db.Products.Add(new Product
            {
                Id = Guid.NewGuid(),
                Name = name,
                Price = price,
                CreatedAtUtc = DateTime.UtcNow
            });

            await db.SaveChangesAsync(ct);
            return db.Products.OrderByDescending(x => x.CreatedAtUtc).Select(x => x.Id).First();
        }, ct);

    public Task DeleteAsync(Guid id, CancellationToken ct) =>
        factory.RunWithDbContext(async db =>
        {
            var entity = await db.Products.FirstOrDefaultAsync(x => x.Id == id, ct);
            if (entity is null) return;

            db.Products.Remove(entity);
            await db.SaveChangesAsync(ct);
        }, ct);
}
```

---

## Demo data (seed on startup)

`DbSeeder`:

* runs migrations
* seeds only if `Products` is empty

Call it at startup in `Program.cs`:

```csharp
using (var scope = app.Services.CreateScope())
{
    var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<CustomAppDbContext>>();
    await DbSeeder.SeedAsync(factory);
}
```


```
```
