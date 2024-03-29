﻿namespace KingMigrations.Helpers;

internal static class CommentLineHandler
{
    public static void ParseCommentData(string line, Migration migration)
    {
        var idMatch = RegularExpressions.Id.Match(line);
        if (idMatch.Success && long.TryParse(idMatch.Groups["id"].Value, out var id))
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

        var enabledMatch = RegularExpressions.Enabled.Match(line);
        if (enabledMatch.Success && bool.TryParse(enabledMatch.Groups["enabled"].Value, out var enabled))
        {
            migration.Enabled = enabled;
            return;
        }
    }
}
