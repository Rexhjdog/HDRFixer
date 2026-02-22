using System.Diagnostics;
using HDRFixer.Core.Settings;
using Xunit;
using Xunit.Abstractions;

namespace HDRFixer.Core.Tests.Settings;

public class SettingsPerformanceTests
{
    private readonly ITestOutputHelper _output;

    public SettingsPerformanceTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void MeasureSynchronousLoadSave()
    {
        string tempDir = Path.Combine(Path.GetTempPath(), "hdrfixer_perf_test_sync_" + Guid.NewGuid());
        Directory.CreateDirectory(tempDir);
        var mgr = new SettingsManager(tempDir);
        var settings = new AppSettings();

        // Warmup
        mgr.Save(settings);
        mgr.Load();

        var sw = Stopwatch.StartNew();
        for (int i = 0; i < 100; i++)
        {
            mgr.Save(settings);
            mgr.Load();
        }
        sw.Stop();

        _output.WriteLine($"Synchronous 100 iterations: {sw.ElapsedMilliseconds}ms");

        try
        {
            Directory.Delete(tempDir, recursive: true);
        }
        catch { }
    }

    [Fact]
    public async Task MeasureAsynchronousLoadSave()
    {
        string tempDir = Path.Combine(Path.GetTempPath(), "hdrfixer_perf_test_async_" + Guid.NewGuid());
        Directory.CreateDirectory(tempDir);
        var mgr = new SettingsManager(tempDir);
        var settings = new AppSettings();

        // Warmup
        await mgr.SaveAsync(settings);
        await mgr.LoadAsync();

        var sw = Stopwatch.StartNew();
        for (int i = 0; i < 100; i++)
        {
            await mgr.SaveAsync(settings);
            await mgr.LoadAsync();
        }
        sw.Stop();

        _output.WriteLine($"Asynchronous 100 iterations: {sw.ElapsedMilliseconds}ms");

        try
        {
            Directory.Delete(tempDir, recursive: true);
        }
        catch { }
    }
}
