namespace KingMigrations.MigrationSources;

public class CompositeMigrationSource : IMigrationSource
{
    private readonly List<IMigrationSource> _migrationSources;

    public CompositeMigrationSource()
    {
        _migrationSources = new List<IMigrationSource>();
    }

    public void AddMigrationSource(IMigrationSource source)
    {
        _migrationSources.Add(source);
    }

    public async Task<IReadOnlyList<Migration>> GetMigrationsAsync()
    {
        var migrations = new List<Migration>();

        foreach (var source in _migrationSources)
        {
            foreach (var migration in await source.GetMigrationsAsync().ConfigureAwait(false))
            {
                migrations.Add(migration);
            }
        }

        return migrations.OrderBy(x => x.Id).ToArray();
    }
}
