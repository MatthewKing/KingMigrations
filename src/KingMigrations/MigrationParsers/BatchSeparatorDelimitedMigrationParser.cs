namespace KingMigrations.MigrationParsers;

/// <summary>
/// An implementation of <see cref="IMigrationParser"/> that uses a batch separator to delimit commands.
/// </summary>
public class BatchSeparatorDelimitedMigrationParser : IMigrationParser
{
    /// <summary>
    /// Gets or sets the batch separator to use.
    /// </summary>
    public static string BatchSeparator { get; set; } = "GO";

    /// <summary>
    /// Parses a migration definition from the specified text reader.
    /// </summary>
    /// <param name="reader">The text reader.</param>
    /// <returns>
    /// A task that represents the asynchronous parse operation.
    /// The task result contains the migration definition.
    /// </returns>
    public async Task<Migration> ParseMigrationAsync(TextReader reader)
    {
        var migration = new Migration();

        var linesInBatch = new List<string>();

        while (true)
        {
            var line = (await reader.ReadLineAsync().ConfigureAwait(false))?.Trim();
            if (line is null)
            {
                break;
            }

            if (string.Equals(line, BatchSeparator, StringComparison.OrdinalIgnoreCase))
            {
                var command = string.Join(Environment.NewLine, linesInBatch);
                migration.Commands.Add(command);

                linesInBatch.Clear();

                continue;
            }

            if (line.StartsWith("--"))
            {
                var idMatch = RegularExpressions.Id.Match(line);
                if (idMatch.Success && int.TryParse(idMatch.Groups["id"].Value, out var id))
                {
                    migration.Id = id;
                    continue;
                }

                var descriptionMatch = RegularExpressions.Description.Match(line);
                if (descriptionMatch.Success)
                {
                    migration.Description = descriptionMatch.Groups["description"].Value;
                    continue;
                }

                continue;
            }

            linesInBatch.Add(line);
        }

        return migration;
    }
}
