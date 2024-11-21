# Serilog.Sinks.AsyncFile

An advanced file sink for Serilog that writes events to files in plain text.

### Features
* Embedded asynchronous log writing using the `Channel<T>` api
* Control over the channel capacity
* Advanced rolling policies
  * Rolling by file size
  * Rolling on startup
  * Change the rolled file name format
  * Roll to archive folder
  * Delete old rolled files by age

More features and ideas will be welcomed! Just log an issue.

### Getting started

Install the [Serilog.Sinks.AsyncFile](https://www.nuget.org/packages/Serilog.Sinks.AsyncFile/) package from NuGet:

```powershell
dotnet add package Serilog.Sinks.AsyncFile
```

To configure the sink in C# code, call `WriteTo.AsyncFile()` during logger configuration:

```csharp
var log = new LoggerConfiguration()
    .WriteTo.AsyncFile("log.txt")
    .CreateLogger();
```

This will append the time period to the filename, creating a file set like:

```
log.txt
2024-11-18-20-39-25_log.txt
2024-11-18-20-47-41_log.txt
2024-11-18-20-56-12_log.txt
```

### Rolling policies

#### Rolling by file size
The file sink can roll files based on a maximum size in bytes. The default value if not specify is 50 MB.
To roll files when they reach a certain size, specify
`RollingPolicyOptions.FileSizeLimitBytes`:

```csharp
    .WriteTo.AsyncFile("log.txt", new RollingPolicyOptions
    {
        FileSizeLimitBytes = 100 * 1024 * 1024
    })
```

This will create a new file when the current one reaches 100 MB.

#### Rolling on startup
The file sink can roll files on startup. To roll files when the application starts, specify
`RollingPolicyOptions.RollOnStartup`:

```csharp
    .WriteTo.AsyncFile("log.txt", new RollingPolicyOptions
    {
        RollOnStartup = true
    })
```

This will create a new file when the application starts.

#### Change the rolled file name format
The sink can roll files with a specific file name format. To roll files with a specific file name format, specify
`RollingPolicyOptions.RollingFileNameFormat` (Default: "{0:yyyy-MM-dd-HH-mm-ss}_{1}{2}"):

- 0: The timestamp of the log event
- 1: The original file name without extension
- 2: The original file extension (with the dot)

```csharp
    .WriteTo.AsyncFile("log.txt", new RollingPolicyOptions
    {
        RollingFileNameFormat = "{0:MM-dd-yyyy-HH-mm-ss}_{1}.archived{2}"
    })
```

#### Roll to archive folder
The file sink can roll files to an archive folder. To roll files to an archive folder, specify
`RollingPolicyOptions.RollToArchiveFolder` (Default: false) and `RollingPolicyOptions.ArchiveFolderName` (Default: "Archive"):

```csharp
    .WriteTo.AsyncFile("log.txt", new RollingPolicyOptions
    {
        RollToArchiveFolder = true,
        ArchiveFolderName = "old"
    })
```

This will create a new file in the archive folder when the current one reaches the maximum size.

#### Delete old rolled files by age
The file sink can delete old rolled files by age. To delete old rolled files by age, specify
`RollingPolicyOptions.RollingRetentionDays` (Default: 14):

```csharp
    .WriteTo.AsyncFile("log.txt", new RollingPolicyOptions
    {
        RollingRetentionDays = 30
    })
```

This will delete old rolled files by age.

#### Age check interval
The file sink can check for old rolled files in logs or archive folder by age at a specified interval. To check for old rolled files by age at a specified interval, specify
`RollingPolicyOptions.AgeCheckInterval` (Default: 1 hour):

```csharp
    .WriteTo.AsyncFile("log.txt", new RollingPolicyOptions
    {
        AgeCheckInterval = TimeSpan.FromHours(2)
    })
```

### Controlling event formatting

The file sink creates events in a fixed text format by default:

```
2018-07-06 09:02:17.148 +10:00 [INF] HTTP GET / responded 200 in 1994 ms
```

The format is controlled using an _output template_, which the file configuration method accepts as an `outputTemplate`
parameter.

The default format above corresponds to an output template like:

```csharp
  .WriteTo.File("log.txt",
    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
```

_Copyright &copy; 2024 by Yoav Melamed - Provided under
the [Apache License, Version 2.0](http://apache.org/licenses/LICENSE-2.0.html)._
