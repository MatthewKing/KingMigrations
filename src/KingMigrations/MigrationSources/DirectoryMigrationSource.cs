namespace KingMigrations.MigrationSources;

public class DirectoryMigrationSource : FileBasedMigrationSource, IMigrationSource
{
    private readonly string _directory;

    public DirectoryMigrationSource(string directory)
    {
        _directory = directory;
    }

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
