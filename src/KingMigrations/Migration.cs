namespace KingMigrations;

/// <summary>
/// Database migration definition.
/// </summary>
public class Migration
{
    /// <summary>
    /// Gets or sets the unique identifier for the migration.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets a description for the migration.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets the list of commands that comprise the migration.
    /// </summary>
    public IList<string> Commands { get; } = new List<string>();
}
