using ProductivityTools.AlibabaCloud.App;
using ProductivityTools.MasterConfiguration;

namespace ProductivityTools.AlibabaCloud.NetCoreService
{
    public class WindowsBackgroundService : BackgroundService
    {
        private readonly ILogger<WindowsBackgroundService> _logger;

        public WindowsBackgroundService()
        {
         
        }

        public async Task OnDebug(CancellationToken stoppingToken)
        {
            await ExecuteAsync(stoppingToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .AddMasterConfiguration(force: true)
                .Build();

            var r = configuration["Region"];

            Application application = new Application(configuration);
            await application.Run(stoppingToken);

            //while (!stoppingToken.IsCancellationRequested)
            //{
             
            //    if (_logger.IsEnabled(LogLevel.Information))
            //    {
            //        _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            //    }
            //    await Task.Delay(1000, stoppingToken);
            //}
        }
    }
}
