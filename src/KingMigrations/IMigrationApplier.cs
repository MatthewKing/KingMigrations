using System.Data.Common;

namespace KingMigrations;

/// <summary>
/// Provides functionality to apply migrations to a database.
/// </summary>
public interface IMigrationApplier
{
    /// <summary>
    /// Applies all of the migrations in the specified migration source to the database.
    /// </summary>
    /// <param name="connection">The database connection.</param>
    /// <param name="migrationSource">The migration source.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task ApplyMigrationsAsync(DbConnection connection, IMigrationSource migrationSource);

    /// <summary>
    /// Applies all of the specified migrations to the database.
    /// </summary>
    /// <param name="connection">The database connection.</param>
    /// <param name="migrations">The migrations to apply.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task ApplyMigrationsAsync(DbConnection connection, IReadOnlyList<Migration> migrations);
}
