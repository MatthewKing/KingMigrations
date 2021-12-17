namespace KingMigrations.MigrationSources;

public abstract class FileBasedMigrationSource : IMigrationSource
{
    protected IList<ConditionalParser> Parsers { get; } = new List<ConditionalParser>();

    public void AddParser(string fileExtension, IMigrationParser parser)
    {
        Parsers.Add(new ConditionalParser(fileName => fileName?.EndsWith(fileExtension, StringComparison.OrdinalIgnoreCase) ?? false, parser));
    }

    public void AddParser(Func<string, bool> condition, IMigrationParser parser)
    {
        Parsers.Add(new ConditionalParser(condition, parser));
    }

    public abstract Task<IReadOnlyList<Migration>> GetMigrationsAsync();

    protected sealed class ConditionalParser
    {
        public Func<string, bool> Condition { get; }
        public IMigrationParser Parser { get; }

        public ConditionalParser(Func<string, bool> condition, IMigrationParser parser)
        {
            Condition = condition;
            Parser = parser;
        }

        public void Deconstruct(out Func<string, bool> condition, out IMigrationParser parser)
        {
            condition = Condition;
            parser = Parser;
        }
    }
}
