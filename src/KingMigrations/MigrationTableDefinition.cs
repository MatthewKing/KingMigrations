namespace KingMigrations;

public class MigrationTableDefinition
{
    public string? TableSchema { get; set; }
    public string? TableName { get; set; }
    public string? IdColumnName { get; set; }
    public string? DescriptionColumnName { get; set; }
    public string? TimestampColumnName { get; set; }
}
