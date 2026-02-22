using HDRFixer.Core.Ipc;
using HDRFixer.Core.Fixes;
using HDRFixer.Core.Settings;
using HDRFixer.Core.OledProtection;

namespace HDRFixer.Service;

public class HdrServiceWorker : BackgroundService
{
    private readonly ILogger<HdrServiceWorker> _logger;
    private readonly IpcServer _ipcServer;
    private readonly FixEngine _fixEngine;
    private readonly SettingsManager _settingsManager;
    private readonly OledGuardian _oledGuardian;

    public HdrServiceWorker(ILogger<HdrServiceWorker> logger)
    {
        _logger = logger;
        _ipcServer = new IpcServer();
        _fixEngine = FixEngineFactory.Create();
        _settingsManager = new SettingsManager();

        var settings = _settingsManager.Load();
        var oledSettings = new OledProtectionSettings
        {
            PixelShiftEnabled = settings.OledPixelShiftEnabled,
            AutoHideTaskbar = settings.OledAutoHideTaskbar,
            DarkModeEnforced = settings.OledDarkModeEnforced
        };
        _oledGuardian = new OledGuardian(oledSettings);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("HDRFixer Service starting...");

        _ipcServer.MessageReceived += OnMessageReceived;
        _ipcServer.Start();

        _oledGuardian.ApplyAll();

        _logger.LogInformation("HDRFixer Service started. Monitoring displays and IPC.");

        while (!stoppingToken.IsCancellationRequested)
        {
            var settings = _settingsManager.Load();

            if (settings.EnableFixWatchdog)
            {
                RunWatchdog();
            }

            _oledGuardian.CheckIdleness();

            // Poll every 10 seconds for more responsive OLED protection
            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
        }

        _oledGuardian.RevertAll();
        _ipcServer.Stop();
    }

    private void RunWatchdog()
    {
        var diagnostics = _fixEngine.DiagnoseAll();
        foreach (var (name, status) in diagnostics)
        {
            if (status.State == FixState.NotApplied)
            {
                _logger.LogInformation("Watchdog: Fix '{Name}' was reverted, re-applying...", name);
                var fix = _fixEngine.GetAllFixes().FirstOrDefault(f => f.Name == name);
                fix?.Apply();
            }
        }
    }

    private void OnMessageReceived(IpcMessage message)
    {
        _logger.LogInformation("IPC command received: {Action}", message.Action);
        switch (message.Action)
        {
            case "ApplyAll":
                _fixEngine.ApplyAll();
                break;
            case "RevertAll":
                _fixEngine.RevertAll();
                break;
            case "UpdateOledSettings":
                // Logic to update oled settings from message payload
                break;
        }
    }
}
