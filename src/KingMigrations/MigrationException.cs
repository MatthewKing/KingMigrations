namespace KingMigrations;

/// <summary>
/// Represents an error that occurred during a migration.
/// </summary>
public class MigrationException : Exception
{
    /// <summary>
    /// Gets the migration that failed.
    /// </summary>
    public Migration? Migration { get; }

    /// <summary>
    /// Gets the command that failed.
    /// </summary>
    public string? Command { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="MigrationException"/> class.
    /// </summary>
    public MigrationException()
        : base() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="MigrationException"/> class.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public MigrationException(string message)
        : base(message) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="MigrationException"/> class.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public MigrationException(string message, Exception innerException)
        : base(message, innerException) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="MigrationException"/> class.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    /// <param name="migration">The migration that caused the exception.</param>
    /// <param name="command">The command that caused the exception.</param>
    public MigrationException(string message, Exception innerException, Migration? migration, string? command)
        : base(message, innerException)
    {
        Migration = migration;
        Command = command;
    }
}
