using System.Data.Common;
using KingMigrations.Extensions;

namespace KingMigrations.PostgreSql;

/// <summary>
/// An implementation of <see cref="IMigrationApplier"/> for PostgreSQL databases.
/// </summary>
public class PostgreSqlMigrationApplier : MigrationApplier
{
    /// <summary>
    /// Gets the migration table definition.
    /// </summary>
    public override MigrationTableDefinition TableDefinition { get; } = new MigrationTableDefinition()
    {
        TableSchema = "public",
        TableName = "version_info",
        IdColumnName = "version",
        DescriptionColumnName = "description",
        TimestampColumnName = "applied_on",
    };

    /// <summary>
    /// Returns the migration table status.
    /// </summary>
    /// <param name="connection">The database connection.</param>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// The task result contains the migration table status.
    /// </returns>
    protected override async Task<MigrationTableStatus> GetMigrationTableStatusAsync(DbConnection connection)
    {
        using var getTableInfoCommand = connection.CreateCommand();
        getTableInfoCommand.CommandText = "SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = @TableSchema AND TABLE_NAME = @TableName;";
        getTableInfoCommand.AddParameter("TableSchema", TableDefinition.TableSchema);
        getTableInfoCommand.AddParameter("TableName", TableDefinition.TableName);

        using var getTableInfoReader = await getTableInfoCommand.ExecuteReaderAsync().ConfigureAwait(false);
        var nameColumnOrdinal = getTableInfoReader.GetOrdinal("COLUMN_NAME");

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

    /// <summary>
    /// Creates the migration table.
    /// </summary>
    /// <param name="connection">The database connection.</param>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// </returns>
    protected override async Task CreateMigrationTableAsync(DbConnection connection)
    {
        var commands = new[]
        {
            $"CREATE TABLE \"{TableDefinition.TableSchema}\".\"{TableDefinition.TableName}\" (\"{TableDefinition.IdColumnName}\" INTEGER NOT NULL, \"{TableDefinition.TimestampColumnName}\" TIMESTAMP WITH TIME ZONE NOT NULL, \"{TableDefinition.DescriptionColumnName}\" TEXT);",
            $"CREATE UNIQUE INDEX \"{TableDefinition.TableName}_{TableDefinition.IdColumnName}_idx\" ON \"{TableDefinition.TableSchema}\".\"{TableDefinition.TableName}\" USING btree (\"{TableDefinition.IdColumnName}\" ASC);",
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

    /// <summary>
    /// Checks whether the specified migration has already been applied.
    /// </summary>
    /// <param name="connection">The database connection.</param>
    /// <param name="migration">The migration to check.</param>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// The task result is true if the migration has already been applied; otherwise, false.
    /// </returns>
    protected override async Task<bool> CheckIfMigrationIsAlreadyAppliedAsync(DbConnection connection, Migration migration)
    {
        using var sqlCommand = connection.CreateCommand();
        sqlCommand.CommandText = $"SELECT COUNT(*) FROM \"{TableDefinition.TableSchema}\".\"{TableDefinition.TableName}\" WHERE \"{TableDefinition.IdColumnName}\" = @ID;";
        sqlCommand.AddParameter("ID", migration.Id);

        var result = await sqlCommand.ExecuteScalarAsync().ConfigureAwait(false);

        return Convert.ToByte(result) > 0;
    }

    /// <summary>
    /// Applies the specified migration to the database.
    /// </summary>
    /// <param name="connection">The database connection.</param>
    /// <param name="migration">The migration to apply.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
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
            applyScriptCommand.CommandText = $"INSERT INTO \"{TableDefinition.TableSchema}\".\"{TableDefinition.TableName}\" (\"{TableDefinition.IdColumnName}\", \"{TableDefinition.DescriptionColumnName}\", \"{TableDefinition.TimestampColumnName}\") VALUES (@Id, @Description, @Timestamp);";
            applyScriptCommand.AddParameter("Id", migration.Id);
            applyScriptCommand.AddParameter("Description", migration.Description);
            applyScriptCommand.AddParameter("Timestamp", DateTimeOffset.UtcNow);

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
