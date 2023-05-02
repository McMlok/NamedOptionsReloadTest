using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

internal sealed class Program
{
    private static async Task Main(string[] args)
    {
        await Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((hostingContext, config) =>
            {
                config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            })
            .ConfigureServices((hostContext, services) =>
            {
                //Working Options registration
                services.AddOptions<ConsoleHostOptions>("Test").Bind(hostContext.Configuration.GetSection("Config:Test"));
                
                //Does not work until IOptionsChangeTokenSource manualy added to the container.
                //services.AddOptions<ConsoleHostOptions>("Test").BindConfiguration("Config:Test");
                //services.AddSingleton<IOptionsChangeTokenSource<ConsoleHostOptions>>(new ConfigurationChangeTokenSource<ConsoleHostOptions>("Test", hostContext.Configuration));
                
                services.AddHostedService<ConsoleHostedService>();
            })
            .RunConsoleAsync();
    }
}

internal sealed class ConsoleHostedService : IHostedService
{
    private readonly IOptionsMonitor<ConsoleHostOptions> _options;
     
    public ConsoleHostedService(IOptionsMonitor<ConsoleHostOptions> options)
    {
        _options = options;
        _options.OnChange((opt, optionName) =>
        {
            Console.WriteLine($"ConsoleHostOptions changed actual value '{opt.Name}' from configuration with name '{optionName}'.");
        });
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        var options = _options.Get("Test");
        Console.WriteLine($"Started with option value {options.Name}");
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}

internal class ConsoleHostOptions
{
    public string Name { get; set; } = "ConsoleHost";
}