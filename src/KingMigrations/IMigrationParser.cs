namespace KingMigrations;

public interface IMigrationParser
{
    Task<Migration> ParseMigrationAsync(TextReader reader);
}
