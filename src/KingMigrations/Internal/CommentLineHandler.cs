namespace KingMigrations.Helpers;

internal static class CommentLineHandler
{
    public static void ParseCommentData(string line, Migration migration)
    {
        var idMatch = RegularExpressions.Id.Match(line);
        if (idMatch.Success && int.TryParse(idMatch.Groups["id"].Value, out var id))
        {
            migration.Id = id;
            return;
        }

        var descriptionMatch = RegularExpressions.Description.Match(line);
        if (descriptionMatch.Success)
        {
            migration.Description = descriptionMatch.Groups["description"].Value;
            return;
        }
    }
}
