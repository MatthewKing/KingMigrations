namespace KingMigrations.MigrationParsers;

public class SemicolonDelimitedMigrationParser : IMigrationParser
{
    public async Task<Migration> ParseMigrationAsync(TextReader reader)
    {
        var migration = new Migration();

        var linesInBatch = new List<string>();

        while (true)
        {
            var line = await reader.ReadLineAsync().ConfigureAwait(false);
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

    public static bool IsLineTerminatedWithSemicolon(string line)
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
