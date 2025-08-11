using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = Host.CreateApplicationBuilder(args);

// Configure all logs to go to stderr (stdout is used for the MCP protocol messages).
builder.Logging.AddConsole(o => o.LogToStandardErrorThreshold = LogLevel.Trace);

// Add the MCP services: the transport to use (stdio) and the tools to register.
builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithTools<RandomNumberTools>()
    .WithTools<WeatherTools>();

//var loggerFactory = LoggerFactory.Create(logging => logging.AddConsole());
//var tools = new WeatherTools(loggerFactory.CreateLogger<WeatherTools>());
//var testResult = await tools.GetCurrentWeather("Atyrau", "KZ");
//Console.WriteLine("Test Result: " + testResult);
//return; // exit so MCP doesn't run

await builder.Build().RunAsync();
