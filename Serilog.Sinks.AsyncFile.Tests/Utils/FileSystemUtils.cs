using System.Diagnostics;
using Xunit.Sdk;

namespace Serilog.Sinks.AsyncFile.Tests.Utils;

public class FileSystemUtils : IDisposable
{
    private string? _tempFilePath;
    
    public string GenerateTempFilePath()
    {
        _tempFilePath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        return _tempFilePath;
    }

    public async Task<string> ReadFirstLine(string path)
    {
        var lines = await File.ReadAllLinesAsync(path);
        if (lines.Length == 0)
            throw new XunitException("No lines were written to the file.");

        return lines[0];
    }
    
    public void Dispose()
    {
        try
        {
            if (File.Exists(_tempFilePath))
                File.Delete(_tempFilePath);
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
        }
        
        GC.SuppressFinalize(this);
    }
}