using System.Threading.Tasks;
using KingMigrations.MigrationParsers;
using KingMigrations.MigrationSources;
using KingMigrations.Sqlite;
using Microsoft.Data.Sqlite;
using Xunit;

namespace KingMigrations.Tests;

public class FullExample
{
    [Fact]
    public async Task Test()
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var migrationSource = new DirectoryMigrationSource("Migrations");
        migrationSource.AddParser(".sqlite", new SemicolonDelimitedMigrationParser());

        var migrator = new SqliteMigrationApplier();
        await migrator.ApplyMigrationsAsync(connection, migrationSource);

        TestHelper.AssertThatTableExists(connection, migrator.TableDefinition.TableName);
        TestHelper.AssertThatTableExists(connection, "Table1");
        TestHelper.AssertThatTableExists(connection, "Table2");
        TestHelper.AssertThatTableDoesNotExist(connection, "Table3");
        TestHelper.AssertThatMigrationsHaveBeenApplied(connection, migrator.TableDefinition, await migrationSource.GetMigrationsAsync());
    }
}
