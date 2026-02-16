using Microsoft.EntityFrameworkCore;

using System;

namespace BlazorFactoryDemo.Data;

public static class DbFactoryHelpers
{
    public static async Task<T> RunWithDbContext<T>(this IDbContextFactory<CustomAppDbContext> factory,
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

    public static async Task<T> WithTransaction<TContext, T>(
        this IDbContextFactory<TContext> factory,
        Func<TContext, Task<T>> action,
        CancellationToken ct = default)
        where TContext : DbContext
    {
        await using var db = await factory.CreateDbContextAsync(ct);
        await using var tx = await db.Database.BeginTransactionAsync(ct);

        var result = await action(db);

        await tx.CommitAsync(ct);
        return result;
    }
}
