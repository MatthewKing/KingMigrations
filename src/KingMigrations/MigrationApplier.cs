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

        if (!Validate(migrationsToApply))
        {
            throw new MigrationException("Not all required migrations are present.");
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

        foreach (var migration in migrationsToApply)
        {
            // If the migration has already been applied, skip to the next migration.
            if (await CheckIfMigrationIsAlreadyAppliedAsync(connection, migration).ConfigureAwait(false))
            {
                continue;
            }

            await ApplyMigrationAsync(connection, migration).ConfigureAwait(false);
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
    protected abstract Task<MigrationTableStatus> GetMigrationTableStatusAsync(DbConnection connection);

    /// <summary>
    /// Creates the migration table.
    /// </summary>
    /// <param name="connection">The database connection.</param>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// </returns>
    protected abstract Task CreateMigrationTableAsync(DbConnection connection);

    /// <summary>
    /// Checks whether the specified migration has already been applied.
    /// </summary>
    /// <param name="connection">The database connection.</param>
    /// <param name="migration">The migration to check.</param>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// The task result is true if the migration has already been applied; otherwise, false.
    /// </returns>
    protected abstract Task<bool> CheckIfMigrationIsAlreadyAppliedAsync(DbConnection connection, Migration migration);

    /// <summary>
    /// Applies the specified migration to the database.
    /// </summary>
    /// <param name="connection">The database connection.</param>
    /// <param name="migration">The migration to apply.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    protected abstract Task ApplyMigrationAsync(DbConnection connection, Migration migration);

    /// <summary>
    /// Validate the list of migrations.
    /// </summary>
    /// <param name="migrations">The list of migrations to validate.</param>
    /// <returns>true if the migrations are valid; otherwise, false.</returns>
    protected bool Validate(IReadOnlyList<Migration> migrations)
    {
        // A list of migrations of length n should always have IDs 1..n
        var expectedIds = Enumerable.Range(1, migrations.Count);
        var actualIds = migrations.Select(m => m.Id).ToArray();
        return Enumerable.SequenceEqual(expectedIds, actualIds);
    }
}
