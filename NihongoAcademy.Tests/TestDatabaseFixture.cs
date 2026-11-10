using Microsoft.EntityFrameworkCore;
using NihongoAcademy.Data;

namespace NihongoAcademy.Tests;

/// <summary>
/// Shared LocalDB test database. A dedicated "NihongoAcademyTest" database is
/// dropped and recreated per fixture so tests never touch the developer's real
/// NihongoAcademyDev data and the seeder (which only runs from Program.cs) never fires.
/// </summary>
public class TestDatabaseFixture : IDisposable
{
    public string ConnectionString { get; }

    public TestDatabaseFixture()
    {
        ConnectionString =
            @"Server=(localdb)\mssqllocaldb;Database=NihongoAcademyTest;Trusted_Connection=True;MultipleActiveResultSets=true;";
        using var context = CreateContext();
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();
    }

    public ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(ConnectionString)
            .Options;
        return new ApplicationDbContext(options);
    }

    /// <summary>
    /// Drop and recreate the test database so each test starts from a clean slate.
    /// Tests share one fixture instance, so without this they leak rows into each other.
    /// </summary>
    public void Reset()
    {
        using var context = CreateContext();
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();
    }

    public void Dispose()
    {
        using var context = CreateContext();
        context.Database.EnsureDeleted();
    }
}
