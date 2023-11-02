using System.Text.RegularExpressions;

namespace KingMigrations.Helpers;

internal static class RegularExpressions
{
    public static Regex Id { get; } = new Regex("-- id: (?<id>.*)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public static Regex Description { get; } = new Regex("-- description: (?<description>.*)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
}
