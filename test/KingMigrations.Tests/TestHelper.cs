using System;
using System.Collections.Generic;
using System.Data.Common;
using KingMigrations.Extensions;
using Microsoft.Data.Sqlite;
using Xunit;

namespace KingMigrations.Tests;

internal static class TestHelper
{
    public static void AssertThatTableExists(SqliteConnection connection, string? table)
    {
        var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type = 'table' AND name = @TableName;";
        command.AddParameter("TableName", table);

        var count = Convert.ToInt32(command.ExecuteScalar());
        var exists = count > 0;

        Assert.True(exists);
    }

    public static void AssertThatTableDoesNotExist(SqliteConnection connection, string? table)
    {
        var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type = 'table' AND name = @TableName;";
        command.AddParameter("TableName", table);

        var count = Convert.ToInt32(command.ExecuteScalar());
        var exists = count > 0;

        Assert.False(exists);
    }

    public static void AssertThatMigrationsHaveBeenApplied(DbConnection connection, MigrationTableDefinition migrationTableDefinition, IEnumerable<Migration> migrations)
    {
        foreach (var migration in migrations)
        {
            var command = connection.CreateCommand();
            command.CommandText = $"SELECT \"{migrationTableDefinition.IdColumnName}\" FROM \"{migrationTableDefinition.TableName}\" WHERE \"{migrationTableDefinition.IdColumnName}\" = @ID;";
            command.AddParameter("ID", migration.Id);

            var expectedResult = migration.Id;
            var actualResult = Convert.ToInt64(command.ExecuteScalar());
            Assert.Equal(expectedResult, actualResult);
        }
    }
}
