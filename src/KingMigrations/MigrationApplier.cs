using System.Data.Common;

namespace KingMigrations;

public abstract class MigrationApplier : IMigrationApplier
{
    public abstract MigrationTableDefinition TableDefinition { get; }

    public async Task ApplyMigrationsAsync(DbConnection connection, IMigrationSource migrationSource)
    {
        var migrations = await migrationSource.GetMigrationsAsync().ConfigureAwait(false);
        await ApplyMigrationsAsync(connection, migrations).ConfigureAwait(false);
    }

    public async Task ApplyMigrationsAsync(DbConnection connection, IReadOnlyList<Migration> migrations)
    {
        var migrationsInOrder = migrations.OrderBy(x => x.Id).ToArray();
        if (!Validate(migrationsInOrder))
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

        foreach (var migration in migrationsInOrder)
        {
            // If the migration has already been applied, skip to the next migration.
            if (await CheckIfMigrationIsAlreadyAppliedAsync(connection, migration).ConfigureAwait(false))
            {
                continue;
            }

            await ApplyMigrationAsync(connection, migration).ConfigureAwait(false);
        }
    }

    protected abstract Task<MigrationTableStatus> GetMigrationTableStatusAsync(DbConnection connection);
    protected abstract Task CreateMigrationTableAsync(DbConnection connection);
    protected abstract Task<bool> CheckIfMigrationIsAlreadyAppliedAsync(DbConnection connection, Migration migration);
    protected abstract Task ApplyMigrationAsync(DbConnection connection, Migration migration);

    protected bool Validate(IReadOnlyList<Migration> migrations)
    {
        // A list of migrations of length n should always have IDs 1..n
        var expectedIds = Enumerable.Range(1, migrations.Count);
        var actualIds = migrations.Select(m => m.Id).ToArray();
        return Enumerable.SequenceEqual(expectedIds, actualIds);
    }
}
