using System.Reflection;

namespace KingMigrations.MigrationSources;

public class AssemblyResourceMigrationSource : FileBasedMigrationSource, IMigrationSource
{
    private readonly Assembly _assembly;

    public AssemblyResourceMigrationSource(Assembly assembly)
    {
        _assembly = assembly;
    }

    public override async Task<IReadOnlyList<Migration>> GetMigrationsAsync()
    {
        var migrations = new List<Migration>();

        var resourceNames = _assembly.GetManifestResourceNames();
        foreach (var resourceName in resourceNames)
        {
            foreach (var (condition, parser) in Parsers)
            {
                if (condition.Invoke(resourceName))
                {
                    using var stream = _assembly.GetManifestResourceStream(resourceName);
                    if (stream != null)
                    {
                        using var reader = new StreamReader(stream);
                        var migration = await parser.ParseMigrationAsync(reader).ConfigureAwait(false);
                        migrations.Add(migration);

                        continue;
                    }
                }
            }
        }

        return migrations.OrderBy(x => x.Id).ToArray();
    }
}
