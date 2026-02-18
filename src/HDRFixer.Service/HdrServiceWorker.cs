using HDRFixer.Core.Ipc;
using HDRFixer.Core.Fixes;
using HDRFixer.Core.Settings;

namespace HDRFixer.Service;

public class HdrServiceWorker : BackgroundService
{
    private readonly ILogger<HdrServiceWorker> _logger;
    private readonly IpcServer _ipcServer;
    private readonly FixEngine _fixEngine;
    private readonly SettingsManager _settings;

    public HdrServiceWorker(ILogger<HdrServiceWorker> logger)
    {
        _logger = logger;
        _ipcServer = new IpcServer();
        _fixEngine = FixEngineFactory.Create();
        _settings = new SettingsManager();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("HDRFixer Service starting...");
        _ipcServer.MessageReceived += OnMessageReceived;
        _ipcServer.Start();
        _logger.LogInformation("HDRFixer Service started. IPC listening.");

        var settings = _settings.Load();
        while (!stoppingToken.IsCancellationRequested)
        {
            if (settings.EnableFixWatchdog)
            {
                var diagnostics = _fixEngine.DiagnoseAll();
                foreach (var (name, status) in diagnostics)
                {
                    if (status.State == FixState.Error)
                    {
                        _logger.LogWarning("Fix '{Name}' in error state, re-applying...", name);
                        _fixEngine.GetAllFixes().FirstOrDefault(f => f.Name == name)?.Apply();
                    }
                }
            }
            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        }
        _ipcServer.Stop();
    }

    private void OnMessageReceived(IpcMessage message)
    {
        _logger.LogInformation("IPC command: {Action}", message.Action);
        switch (message.Action)
        {
            case "ApplyAll":  _fixEngine.ApplyAll();    break;
            case "RevertAll": _fixEngine.RevertAll();   break;
            case "Diagnose":  _fixEngine.DiagnoseAll(); break;
        }
    }
}