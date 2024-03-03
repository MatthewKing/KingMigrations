using System.Data.Common;

namespace KingMigrations;

/// <summary>
/// A base implementation of <see cref="IMigrationApplier"/>.
/// </summary>
public abstract class MigrationApplier : IMigrationApplier
{
    /// <summary>
    /// Gets the migration table definition.
    /// </summary>
    public abstract MigrationTableDefinition TableDefinition { get; }

    /// <summary>
    /// Applies all of the migrations in the specified migration source to the database.
    /// </summary>
    /// <param name="connection">The database connection.</param>
    /// <param name="migrationSource">The migration source.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task ApplyMigrationsAsync(DbConnection connection, IMigrationSource migrationSource)
    {
        var migrations = await migrationSource.GetMigrationsAsync().ConfigureAwait(false);
        await ApplyMigrationsAsync(connection, migrations).ConfigureAwait(false);
    }

    /// <summary>
    /// Applies all of the specified migrations to the database.
    /// </summary>
    /// <param name="connection">The database connection.</param>
    /// <param name="migrations">The migrations to apply.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task ApplyMigrationsAsync(DbConnection connection, IReadOnlyList<Migration> migrations)
    {
        var migrationsToApply = migrations
            .OrderBy(x => x.Id)
            .TakeWhile(x => x.Enabled)
            .ToArray();

        if (migrationsToApply.Length == 0)
        {
            throw new MigrationException("No migrations available.");
        }

        var status = await GetMigrationTableStatusAsync(connection).ConfigureAwait(false);
        if (!status.IsPresent)
        {
            await CreateMigrationTableAsync(connection).ConfigureAwait(false);
        }
        else if (!status.HasCorrectColumns)
        {
            throw new MigrationException($"Migration table '{TableDefinition}' does not have the required schema.");
        }

        var mostRecentMigrationId = await GetMostRecentMigrationId(connection).ConfigureAwait(false);
        foreach (var migration in migrationsToApply.Where(x => (!mostRecentMigrationId.HasValue || mostRecentMigrationId.Value < x.Id)))
        {
            await ApplyMigrationAsync(connection, migration).ConfigureAwait(false);

            // After applying a migration, we do need to re-check the most recent migration ID, just in case
            // there was a manual command in the migration itself that caused an update beyond what would
            // be usually expected.
            mostRecentMigrationId = await GetMostRecentMigrationId(connection).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Returns the migration table status.
    /// </summary>
    /// <param name="connection">The database connection.</param>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// The task result contains the migration table status.
    /// </returns>
    internal protected abstract Task<MigrationTableStatus> GetMigrationTableStatusAsync(DbConnection connection);

    /// <summary>
    /// Creates the migration table.
    /// </summary>
    /// <param name="connection">The database connection.</param>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// </returns>
    internal protected abstract Task CreateMigrationTableAsync(DbConnection connection);

    /// <summary>
    /// Returns the most recently applied migration ID.
    /// </summary>
    /// <param name="connection">The database connection.</param>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// The task result contains the migration ID.
    /// </returns>
    internal protected abstract Task<long?> GetMostRecentMigrationId(DbConnection connection);

    /// <summary>
    /// Applies the specified migration to the database.
    /// </summary>
    /// <param name="connection">The database connection.</param>
    /// <param name="migration">The migration to apply.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    internal protected abstract Task ApplyMigrationAsync(DbConnection connection, Migration migration);
}
