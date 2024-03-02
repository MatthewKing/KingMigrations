namespace KingMigrations;

/// <summary>
/// Database migration definition.
/// </summary>
public class Migration
{
    /// <summary>
    /// Gets or sets the unique identifier for the migration.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Gets or sets a description for the migration.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets a value determining whether the migration is enabled or not.
    /// Default is true.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Gets the list of commands that comprise the migration.
    /// </summary>
    public IList<string> Commands { get; } = new List<string>();
}
