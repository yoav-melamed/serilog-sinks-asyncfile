namespace Serilog.Sinks.AsyncFile;

/// <summary>
/// Rolling policy options for the FileEx sink.
/// </summary>
public record RollingPolicyOptions
{
    /// <summary>
    /// The maximum size, in bytes, to which any single log file will be allowed to grow before rolling.
    /// Default: 50 MB
    /// </summary>
    public int FileSizeLimitBytes { get; init; } = 50 * 1024 * 1024;
    
    /// <summary>
    /// Weather to roll log files to archive folder or keep them in the same folder.
    /// Default: false
    /// </summary>
    public bool RollToArchiveFolder { get; init; }
    
    /// <summary>
    /// The number of days rolled log files will be retained in logs or archive folder.
    /// Default: 14 days
    /// </summary>
    public int RollingRetentionDays { get; init; } = 14;
    
    /// <summary>
    /// The interval, in seconds, at which the rolling policy will check for old log files to retain.
    /// Default: 3600 seconds (1 hour)
    /// </summary>
    public int AgeCheckInterval { get; init; } = 3600;
    
    /// <summary>
    /// Weather to roll log files on app startup.
    /// Default: false
    /// </summary>
    public bool RollOnStartup { get; init; }
    
    /// <summary>
    /// The name of the archive folder where rolled log files will be stored.
    /// Only used if RollToArchiveFolder is set to true.
    /// Default: "Archive"
    /// </summary>
    public string ArchiveFolderName { get; init; } = "Archive";
    
    /// <summary>
    /// The format of the rolled log file name.
    /// Default: "{0:yyyy-MM-dd-HH-mm-ss}_{1}{2}"
    /// </summary>
    public string RollingFileNameFormat { get; init; } = "{0:yyyy-MM-dd-HH-mm-ss}_{1}{2}";
}