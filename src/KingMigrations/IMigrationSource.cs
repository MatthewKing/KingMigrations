namespace KingMigrations;

/// <summary>
/// Provides functionality to get a list of migration definitions.
/// </summary>
public interface IMigrationSource
{
    /// <summary>
    /// Gets a list of migration definitions.
    /// </summary>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// The task result contains a list of migration definitions.
    /// </returns>
    Task<IReadOnlyList<Migration>> GetMigrationsAsync();
}
