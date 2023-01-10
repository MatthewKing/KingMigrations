namespace KingMigrations;

/// <summary>
/// Provides functionality to parse migration definitions.
/// </summary>
public interface IMigrationParser
{
    /// <summary>
    /// Parses a migration definition from the specified text reader.
    /// </summary>
    /// <param name="reader">The text reader.</param>
    /// <returns>
    /// A task that represents the asynchronous parse operation.
    /// The task result contains the migration definition.
    /// </returns>
    Task<Migration> ParseMigrationAsync(TextReader reader);
}
