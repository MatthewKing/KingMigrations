using System.Data.Common;

namespace KingMigrations;

public interface IMigrationApplier
{
    Task ApplyMigrationsAsync(DbConnection connection, IMigrationSource migrationSource);
    Task ApplyMigrationsAsync(DbConnection connection, IReadOnlyList<Migration> migrations);
}
