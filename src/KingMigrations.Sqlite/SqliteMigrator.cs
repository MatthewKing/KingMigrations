using System.Data.Common;
using KingMigrations.Extensions;

namespace KingMigrations.Sqlite;

public class SqliteMigrator : Migrator
{
    public override MigrationTableDefinition TableDefinition { get; } = new MigrationTableDefinition()
    {
        TableSchema = null,
        TableName = "VersionInfo",
        IdColumnName = "Version",
        DescriptionColumnName = "Description",
        TimestampColumnName = "AppliedOn",
    };

    protected override async Task<MigrationTableStatus> GetMigrationTableStatusAsync(DbConnection connection)
    {
        using var getTableInfoCommand = connection.CreateCommand();
        getTableInfoCommand.CommandText = $"PRAGMA table_info({TableDefinition.TableName});";

        using var getTableInfoReader = await getTableInfoCommand.ExecuteReaderAsync().ConfigureAwait(false);
        var nameColumnOrdinal = getTableInfoReader.GetOrdinal("name");

        var columnNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        while (await getTableInfoReader.ReadAsync().ConfigureAwait(false))
        {
            var columnName = getTableInfoReader.GetString(nameColumnOrdinal);
            columnNames.Add(columnName);
        }

        var status = new MigrationTableStatus();
        status.IsPresent = columnNames.Any();
        status.HasCorrectColumns = TableDefinition.IdColumnName != null
                              && columnNames.Contains(TableDefinition.IdColumnName)
                              && TableDefinition.DescriptionColumnName != null
                              && columnNames.Contains(TableDefinition.DescriptionColumnName)
                              && TableDefinition.TimestampColumnName != null
                              && columnNames.Contains(TableDefinition.TimestampColumnName);

        return status;
    }

    protected override async Task CreateMigrationTableAsync(DbConnection connection)
    {
        var commands = new[]
        {
            $"CREATE TABLE \"{TableDefinition.TableName}\" (\"{TableDefinition.IdColumnName}\" INTEGER NOT NULL, \"{TableDefinition.TimestampColumnName}\" TEXT NOT NULL, \"{TableDefinition.DescriptionColumnName}\" TEXT);",
            $"CREATE UNIQUE INDEX \"UC_{TableDefinition.TableName}\" ON \"{TableDefinition.TableName}\" (\"{TableDefinition.IdColumnName}\" ASC);",
        };

        using var transaction = connection.BeginTransaction();

        foreach (var command in commands)
        {
            try
            {
                using var sqlCommand = connection.CreateCommand();
                sqlCommand.Transaction = transaction;
                sqlCommand.CommandText = command;

                await sqlCommand.ExecuteNonQueryAsync().ConfigureAwait(false);
            }
            catch (DbException ex)
            {
                transaction.Rollback();

                throw new MigrationException("Error creating migration table.", ex, null, command);
            }
        }

        transaction.Commit();
    }

    protected override async Task<bool> CheckIfMigrationIsAlreadyAppliedAsync(DbConnection connection, Migration migration)
    {
        using var sqlCommand = connection.CreateCommand();
        sqlCommand.CommandText = $"SELECT COUNT(*) FROM \"{TableDefinition.TableName}\" WHERE \"{TableDefinition.IdColumnName}\" = @ID;";
        sqlCommand.AddParameter("ID", migration.Id);

        var result = await sqlCommand.ExecuteScalarAsync().ConfigureAwait(false);

        return Convert.ToByte(result) > 0;
    }

    protected override async Task ApplyMigrationAsync(DbConnection connection, Migration migration)
    {
        using var transaction = connection.BeginTransaction();

        foreach (var command in migration.Commands)
        {
            try
            {
                using var sqlCommand = connection.CreateCommand();
                sqlCommand.Transaction = transaction;
                sqlCommand.CommandText = command;

                await sqlCommand.ExecuteNonQueryAsync().ConfigureAwait(false);
            }
            catch (DbException ex)
            {
                transaction.Rollback();

                throw new MigrationException("Error applying migration.", ex, migration, command);
            }
        }

        try
        {
            using var applyScriptCommand = connection.CreateCommand();
            applyScriptCommand.Transaction = transaction;
            applyScriptCommand.CommandText = $"INSERT INTO \"{TableDefinition.TableName}\" (\"{TableDefinition.IdColumnName}\", \"{TableDefinition.DescriptionColumnName}\", \"{TableDefinition.TimestampColumnName}\") VALUES (@Id, @Description, @Timestamp);";
            applyScriptCommand.AddParameter("Id", migration.Id);
            applyScriptCommand.AddParameter("Description", migration.Description);
            applyScriptCommand.AddParameter("Timestamp", DateTimeOffset.Now.ToString("yyyy-MM-dd HH:mm:ss.FFFFFFFzzz"));

            await applyScriptCommand.ExecuteNonQueryAsync().ConfigureAwait(false);
        }
        catch (DbException ex)
        {
            transaction.Rollback();

            throw new MigrationException("Error updating migration metadata table.", ex, migration, null);
        }

        transaction.Commit();
    }
}
