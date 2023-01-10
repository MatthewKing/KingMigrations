namespace KingMigrations;

/// <summary>
/// Defines the properties of the migration table.
/// </summary>
public class MigrationTableDefinition
{
    /// <summary>
    /// Gets or sets the table schema.
    /// </summary>
    public string? TableSchema { get; set; }

    /// <summary>
    /// Gets or sets the table name.
    /// </summary>
    public string? TableName { get; set; }

    /// <summary>
    /// Gets or sets the name of the ID column.
    /// </summary>
    public string? IdColumnName { get; set; }

    /// <summary>
    /// Gets or sets the name of the description column.
    /// </summary>
    public string? DescriptionColumnName { get; set; }

    /// <summary>
    /// Gets or sets the name of the timestamp column.
    /// </summary>
    public string? TimestampColumnName { get; set; }
}
