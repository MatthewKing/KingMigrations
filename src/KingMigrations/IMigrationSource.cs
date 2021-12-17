namespace KingMigrations;

public interface IMigrationSource
{
    Task<IReadOnlyList<Migration>> GetMigrationsAsync();
}
