namespace KingMigrations.MigrationParsers;

public class BatchSeparatorDelimitedMigrationParser : IMigrationParser
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

            if (string.Equals(line, "GO", StringComparison.OrdinalIgnoreCase))
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
