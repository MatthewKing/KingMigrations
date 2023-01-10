namespace KingMigrations.MigrationSources;

/// <summary>
/// An implementation of <see cref="IMigrationSource"/> that reads migration definitions from files in a directory.
/// </summary>
public class DirectoryMigrationSource : FileBasedMigrationSource, IMigrationSource
{
    /// <summary>
    /// The path of the directory.
    /// </summary>
    private readonly string _directory;

    /// <summary>
    /// Initializes a new instance of the <see cref="DirectoryMigrationSource"/> class.
    /// </summary>
    /// <param name="directory">The path of the directory.</param>
    public DirectoryMigrationSource(string directory)
    {
        _directory = directory;
    }

    /// <summary>
    /// Gets a list of migration definitions.
    /// </summary>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// The task result contains a list of migration definitions.
    /// </returns>
    public override async Task<IReadOnlyList<Migration>> GetMigrationsAsync()
    {
        var migrations = new List<Migration>();

        var files = Directory.GetFiles(_directory);
        foreach (var file in files)
        {
            foreach (var (condition, parser) in Parsers)
            {
                if (condition.Invoke(file))
                {
                    using var stream = File.Open(file, FileMode.Open, FileAccess.Read);
                    using var reader = new StreamReader(stream);
                    var migration = await parser.ParseMigrationAsync(reader);
                    migrations.Add(migration);

                    continue;
                }
            }
        }

        return migrations.OrderBy(x => x.Id).ToArray();
    }
}
