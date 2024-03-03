using System.Collections.Generic;
using System.Threading.Tasks;
using KingMigrations.Sqlite;
using Microsoft.Data.Sqlite;
using Xunit;

namespace KingMigrations.Tests;

public class MigrationApplierTests
{
    [Fact]
    public async Task ApplyAllMigrations()
    {
        var migrations = new List<Migration>
        {
            new Migration()
            {
                Id = 1,
                Commands =
                {
                    "CREATE TABLE Table1 (Id INTEGER PRIMARY KEY, Name TEXT);"
                }
            },
            new Migration()
            {
                Id = 2,
                Commands =
                {
                    "CREATE TABLE Table2 (Id INTEGER PRIMARY KEY, Name TEXT);"
                }
            },
            new Migration()
            {
                Id = 3,
                Commands =
                {
                    "CREATE TABLE Table3 (Id INTEGER PRIMARY KEY, Name TEXT);"
                }
            }
        };

        using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var migrator = new SqliteMigrationApplier();
        await migrator.ApplyMigrationsAsync(connection, migrations);

        TestHelper.AssertThatTableExists(connection, migrator.TableDefinition.TableName);
        TestHelper.AssertThatTableExists(connection, "Table1");
        TestHelper.AssertThatTableExists(connection, "Table2");
        TestHelper.AssertThatTableExists(connection, "Table3");
        TestHelper.AssertThatMigrationsHaveBeenApplied(connection, migrator.TableDefinition, migrations);
    }

    [Fact]
    public async Task ApplyOnlyNewerMigrations()
    {
        var migrations = new List<Migration>
        {
            new Migration()
            {
                Id = 1,
                Commands =
                {
                    "CREATE TABLE Table1 (Id INTEGER PRIMARY KEY, Name TEXT);"
                }
            },
            new Migration()
            {
                Id = 2,
                Commands =
                {
                    "CREATE TABLE Table2 (Id INTEGER PRIMARY KEY, Name TEXT);"
                }
            },
            new Migration()
            {
                Id = 3,
                Commands =
                {
                    "CREATE TABLE Table3 (Id INTEGER PRIMARY KEY, Name TEXT);"
                }
            }
        };

        using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var migrator = new SqliteMigrationApplier();

        // Pre-populate the migration table.
        await migrator.CreateMigrationTableAsync(connection);

        // Pre-apply a single migration.
        await migrator.ApplyMigrationAsync(connection, migrations[0]);

        // Now actually run ApplyMigrationsAsync - it should only apply the last two migrations.
        // This is our method under test.
        await migrator.ApplyMigrationsAsync(connection, migrations);

        TestHelper.AssertThatTableExists(connection, migrator.TableDefinition.TableName);
        TestHelper.AssertThatTableExists(connection, "Table1");
        TestHelper.AssertThatTableExists(connection, "Table2");
        TestHelper.AssertThatTableExists(connection, "Table3");
        TestHelper.AssertThatMigrationsHaveBeenApplied(connection, migrator.TableDefinition, migrations);
    }

    [Fact]
    public async Task MigrationHasNoDescription()
    {
        var migrations = new List<Migration>
        {
            new Migration()
            {
                Id = 1,
                Commands =
                {
                    "CREATE TABLE Table1 (Id INTEGER PRIMARY KEY, Name TEXT);"
                }
            }
        };

        using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var migrator = new SqliteMigrationApplier();
        await migrator.ApplyMigrationsAsync(connection, migrations);

        TestHelper.AssertThatTableExists(connection, migrator.TableDefinition.TableName);
        TestHelper.AssertThatTableExists(connection, "Table1");
        TestHelper.AssertThatMigrationsHaveBeenApplied(connection, migrator.TableDefinition, migrations);
    }

    [Fact]
    public async Task MigrationAddsItsOwnEntryToTheMigrationTable()
    {
        var migrations = new List<Migration>
        {
            new Migration()
            {
                Id = 1,
                Description = "This is a regular migration that creates a table.",
                Commands =
                {
                    "CREATE TABLE Table1 (Id INTEGER PRIMARY KEY, Name TEXT);"
                }
            },
            new Migration()
            {
                Id = 2,
                Description = "This migration will add a row to the migration table, flagging migration3 as having already been applied.",
                Commands =
                {
                    "INSERT INTO VersionInfo VALUES (3, '2024-03-03T00:00:00', 'Migration 3 added by migration 2');"
                }
            },
            new Migration()
            {
                Id = 3,
                Description = "This one is meant to create table 3, but it won't get run because migration 2 flagged it as having already being run.",
                Commands =
                {
                    "CREATE TABLE Table3 (Id INTEGER PRIMARY KEY, Name TEXT);"
                }
            }
        };

        using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var migrator = new SqliteMigrationApplier();
        await migrator.ApplyMigrationsAsync(connection, migrations);

        TestHelper.AssertThatTableExists(connection, migrator.TableDefinition.TableName);
        TestHelper.AssertThatTableExists(connection, "Table1");
        TestHelper.AssertThatTableDoesNotExist(connection, "Table3");
        TestHelper.AssertThatMigrationsHaveBeenApplied(connection, migrator.TableDefinition, migrations);
    }
}
