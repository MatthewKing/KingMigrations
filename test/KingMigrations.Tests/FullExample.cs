using System;
using System.Threading.Tasks;
using KingMigrations.Extensions;
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

        var migrator = new SqliteMigrator();
        await migrator.ApplyMigrationsAsync(connection, migrationSource);

        var expectedTables = new[]
        {
            (migrator.TableDefinition.TableName, true),
            ("Table1", true),
            ("Table2", true),
            ("Table3", false),
        };

        foreach (var (tableName, shouldExist) in expectedTables)
        {
            var command = connection.CreateCommand();
            command.CommandText = "SELECT name FROM sqlite_master WHERE type = 'table' AND name = @TableName;";
            command.AddParameter("TableName", tableName);

            var expectedResult = shouldExist ? tableName : null;
            var actualResult = await command.ExecuteScalarAsync();
            Assert.Equal(expectedResult, actualResult);
        }

        foreach (var migration in await migrationSource.GetMigrationsAsync())
        {
            var command = connection.CreateCommand();
            command.CommandText = $"SELECT \"{migrator.TableDefinition.IdColumnName}\" FROM \"{migrator.TableDefinition.TableName}\" WHERE \"{migrator.TableDefinition.IdColumnName}\" = @ID;";
            command.AddParameter("ID", migration.Id);

            var expectedResult = migration.Id;
            var actualResult = Convert.ToInt32(await command.ExecuteScalarAsync());
            Assert.Equal(expectedResult, actualResult);
        }
    }
}
