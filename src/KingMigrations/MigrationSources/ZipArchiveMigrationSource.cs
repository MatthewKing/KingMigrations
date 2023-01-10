using System.IO.Compression;

namespace KingMigrations.MigrationSources;

/// <summary>
/// An implementation of <see cref="IMigrationSource"/> that reads migration definitions from a zip archive.
/// </summary>
public class ZipArchiveMigrationSource : FileBasedMigrationSource, IMigrationSource
{
    /// <summary>
    /// The path of the zip archive.
    /// </summary>
    private readonly string _path;

    /// <summary>
    /// Initializes a new instance of the <see cref="ZipArchiveMigrationSource"/> class.
    /// </summary>
    /// <param name="path">The path of the zip archive.</param>
    public ZipArchiveMigrationSource(string path)
    {
        _path = path;
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

        using var zip = ZipFile.OpenRead(_path);
        foreach (var entry in zip.Entries)
        {
            foreach (var (condition, parser) in Parsers)
            {
                if (condition.Invoke(entry.FullName))
                {
                    using var stream = entry.Open();
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
