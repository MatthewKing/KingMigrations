using System.Reflection;
using System.Threading.Tasks;
using KingMigrations.MigrationParsers;
using KingMigrations.MigrationSources;
using Xunit;

namespace KingMigrations.Tests;

public class MigrationSources
{
    [Fact]
    public async Task DirectoryMigrationSource()
    {
        var migrationSource = new DirectoryMigrationSource("Migrations");
        migrationSource.AddParser(".sqlite", new SemicolonDelimitedMigrationParser());

        var migrations = await migrationSource.GetMigrationsAsync();

        Assert.True(migrations.Count == 3);
        Assert.True(migrations[0].Id == 1);
        Assert.True(migrations[1].Id == 2);
        Assert.True(migrations[2].Id == 3);
    }

    [Fact]
    public async Task ZipArchiveMigrationSource()
    {
        var migrationSource = new ZipArchiveMigrationSource("Migrations/Migrations.zip");
        migrationSource.AddParser(".sqlite", new SemicolonDelimitedMigrationParser());

        var migrations = await migrationSource.GetMigrationsAsync();

        Assert.True(migrations.Count == 3);
        Assert.True(migrations[0].Id == 1);
        Assert.True(migrations[1].Id == 2);
        Assert.True(migrations[2].Id == 3);
    }

    [Fact]
    public async Task AssemblyResourceMigrationSource()
    {
        var migrationSource = new AssemblyResourceMigrationSource(Assembly.GetExecutingAssembly());
        migrationSource.AddParser(".sqlite", new SemicolonDelimitedMigrationParser());

        var migrations = await migrationSource.GetMigrationsAsync();

        Assert.True(migrations.Count == 3);
        Assert.True(migrations[0].Id == 1);
        Assert.True(migrations[1].Id == 2);
        Assert.True(migrations[2].Id == 3);
    }
}
