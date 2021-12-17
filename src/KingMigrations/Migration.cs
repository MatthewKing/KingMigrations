namespace KingMigrations;

public class Migration
{
    public int Id { get; set; }
    public string? Description { get; set; }
    public IList<string> Commands { get; } = new List<string>();
}
