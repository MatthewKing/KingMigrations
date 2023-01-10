using System.Reflection;

namespace KingMigrations.MigrationSources;

/// <summary>
/// An implementation of <see cref="IMigrationSource"/> that reads migration definitions from embedded resources in an assembly.
/// </summary>
public class AssemblyResourceMigrationSource : FileBasedMigrationSource, IMigrationSource
{
    /// <summary>
    /// The assembly to read the migration definitions from.
    /// </summary>
    private readonly Assembly _assembly;

    /// <summary>
    /// Initializes a new instance of the <see cref="AssemblyResourceMigrationSource"/> class.
    /// </summary>
    /// <param name="assembly">The assembly to read the migration definitions from.</param>
    public AssemblyResourceMigrationSource(Assembly assembly)
    {
        _assembly = assembly;
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
