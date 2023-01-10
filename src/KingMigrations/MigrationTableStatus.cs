namespace KingMigrations;

/// <summary>
/// The status of the migration table.
/// </summary>
public class MigrationTableStatus
{
    /// <summary>
    /// Gets or sets a value determining whether the table is present or not.
    /// </summary>
    public bool IsPresent { get; set; }

    /// <summary>
    /// Gets or sets a value determining whether the table has the correct columns or not.
    /// </summary>
    public bool HasCorrectColumns { get; set; }
}
