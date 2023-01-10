namespace KingMigrations.MigrationParsers;

/// <summary>
/// An implementation of <see cref="IMigrationParser"/> that uses semicolons to delimit commands.
/// </summary>
public class SemicolonDelimitedMigrationParser : IMigrationParser
{
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

            if (string.IsNullOrWhiteSpace(line))
            {
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

            if (IsLineTerminatedWithSemicolon(line))
            {
                var command = string.Join(Environment.NewLine, linesInBatch);
                migration.Commands.Add(command);

                linesInBatch.Clear();
            }
        }

        return migration;
    }

    private static bool IsLineTerminatedWithSemicolon(string line)
    {
        for (int i = line.Length - 1; i >= 0; i--)
        {
            if (line[i] == ';')
            {
                return true;
            }
            else if (!char.IsWhiteSpace(line[i]))
            {
                return false;
            }
        }

        return false;
    }
}
