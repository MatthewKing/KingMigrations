namespace KingMigrations.MigrationSources;

/// <summary>
/// An abstract base class for file-based migration sources.
/// </summary>
public abstract class FileBasedMigrationSource : IMigrationSource
{
    /// <summary>
    /// Gets a list of conditional parsers to use.
    /// </summary>
    protected IList<ConditionalParser> Parsers { get; } = new List<ConditionalParser>();

    /// <summary>
    /// Adds a parser to run on all files with the matching file extension.
    /// </summary>
    /// <param name="fileExtension">The file extension.</param>
    /// <param name="parser">The parser to use.</param>
    public void AddParser(string fileExtension, IMigrationParser parser)
    {
        Parsers.Add(new ConditionalParser(fileName => fileName?.EndsWith(fileExtension, StringComparison.OrdinalIgnoreCase) ?? false, parser));
    }

    /// <summary>
    /// Adds a parser to run on all files matching the specified condition.
    /// </summary>
    /// <param name="condition">The condition predicate.</param>
    /// <param name="parser">The parser to use.</param>
    public void AddParser(Func<string, bool> condition, IMigrationParser parser)
    {
        Parsers.Add(new ConditionalParser(condition, parser));
    }

    /// <summary>
    /// Gets a list of migration definitions.
    /// </summary>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// The task result contains a list of migration definitions.
    /// </returns>
    public abstract Task<IReadOnlyList<Migration>> GetMigrationsAsync();

    /// <summary>
    /// Encapsulates a parser and a condition.
    /// </summary>
    protected sealed class ConditionalParser
    {
        /// <summary>
        /// Gets the condition.
        /// </summary>
        public Func<string, bool> Condition { get; }

        /// <summary>
        /// Gets the parser.
        /// </summary>
        public IMigrationParser Parser { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConditionalParser"/> class.
        /// </summary>
        /// <param name="condition">The condition.</param>
        /// <param name="parser">The parser.</param>
        public ConditionalParser(Func<string, bool> condition, IMigrationParser parser)
        {
            Condition = condition;
            Parser = parser;
        }

        /// <summary>
        /// Deconstructs the condition and the parser.
        /// </summary>
        /// <param name="condition">The condition.</param>
        /// <param name="parser">The parser.</param>
        public void Deconstruct(out Func<string, bool> condition, out IMigrationParser parser)
        {
            condition = Condition;
            parser = Parser;
        }
    }
}
