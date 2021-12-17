using System.IO.Compression;

namespace KingMigrations.MigrationSources;

public class ZipArchiveMigrationSource : FileBasedMigrationSource, IMigrationSource
{
    private readonly string _path;

    public ZipArchiveMigrationSource(string path)
    {
        _path = path;
    }

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
