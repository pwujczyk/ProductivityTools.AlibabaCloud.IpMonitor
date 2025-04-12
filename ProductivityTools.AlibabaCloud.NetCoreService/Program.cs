using ProductivityTools.AlibabaCloud.NetCoreService;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddWindowsService(options =>
{
    options.ServiceName = "PT.AlibabaCloud";
});
builder.Services.AddHostedService<WindowsBackgroundService>();

var host = builder.Build();
host.Run();
