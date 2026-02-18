using HDRFixer.Service;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddWindowsService(options => { options.ServiceName = "HDRFixer Service"; });
builder.Services.AddHostedService<HdrServiceWorker>();
var host = builder.Build();
host.Run();
