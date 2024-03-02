# KingMigrations

A really lightweight and simple database migration library. This is not intended to compete with the more advanced migration frameworks; rather, it's just for when you want something very small and very simple to handle some basic database migrations.

## Packages

* [KingMigrations](https://www.nuget.org/packages/KingMigrations)
* [KingMigrations.Sqlite](https://www.nuget.org/packages/KingMigrations.Sqlite)
* [KingMigrations.SqlServer](https://www.nuget.org/packages/KingMigrations.SqlServer)
* [KingMigrations.PostgreSql](https://www.nuget.org/packages/KingMigrations.PostgreSql)

## A quick example

1) Create a migration script (`001.sql`):

```sql
-- Id: 1
-- Description: The first example migration script.

create table table_1 (id integer primary key, value text not null);
create table table_2 (id integer primary key, value text not null);
```

2) Create another migration script (`002.sql`):

```sql
-- Id: 2
-- Description: The second example migration script.

create table table_3 (id integer primary key, value text not null);
create table table_4 (id integer primary key, value text not null);
```

3) Run the migrations:

```csharp
var migrationApplier = new SqliteMigrationApplier();
var migrationSource = new DirectoryMigrationSource("/path/to/your/migration/directory");
migrationSource.AddParser(".sql", new SemicolonDelimitedMigrationParser());

using var connection = InitializeYourSqliteDatabaseConnectionHere();
await connection.OpenAsync();
await migrationApplier.ApplyMigrationsAsync(connection, migrationSource);
```

## Breaking changes from v1 to v2

In v2, I've tried to align the migration table with FluentMigrator (so that users can swap from this library to FluentMigrator, or from FluentMigrator to this library, without too much effort).
As a result, the following breaking changes are present:

* Migration table now stores ID as `bigint` instead of `int`, to support the "timestamp as ID" convention that is present in both FluentMigrator and EFCore.
* Migration table now stores timestamp as a UTC `datetime` / `timestamp` / etc. instead of `datetimeoffset` / `timestamp with time zone` / etc.
* Migration applier no longer checks for IDs in a strict sequence (1, 2, 3, etc.), but rather will simply apply all migration that are newer than the most recently applied migration.

## License and copyright

Copyright Matthew King.
Distributed under the [MIT License](http://opensource.org/licenses/MIT).
Refer to license.txt for more information.
