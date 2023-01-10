namespace KingMigrations.MigrationSources;

/// <summary>
/// An implementation of <see cref="IMigrationSource"/> that combines a number of other sources.
/// </summary>
public class CompositeMigrationSource : IMigrationSource
{
    /// <summary>
    /// The list of sources to use.
    /// </summary>
    private readonly List<IMigrationSource> _migrationSources;

    /// <summary>
    /// Initializes a new instance of the <see cref="CompositeMigrationSource"/> class.
    /// </summary>
    public CompositeMigrationSource()
    {
        _migrationSources = new List<IMigrationSource>();
    }

    /// <summary>
    /// Adds a source.
    /// </summary>
    /// <param name="source">The source to add.</param>
    public void AddMigrationSource(IMigrationSource source)
    {
        _migrationSources.Add(source);
    }

    /// <summary>
    /// Gets a list of migration definitions.
    /// </summary>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// The task result contains a list of migration definitions.
    /// </returns>
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
