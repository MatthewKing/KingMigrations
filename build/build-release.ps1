$root = Resolve-Path (Join-Path $PSScriptRoot "..")
$output = "$root/artifacts"
$projects = @(
    "$root/src/KingMigrations/KingMigrations.csproj",
    "$root/src/KingMigrations.Sqlite/KingMigrations.Sqlite.csproj",
    "$root/src/KingMigrations.SqlServer/KingMigrations.SqlServer.csproj"
)

foreach ($project in $projects) {
    dotnet pack $project --configuration Release --output $output -p:ContinuousIntegrationBuild=true
}
