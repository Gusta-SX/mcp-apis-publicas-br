using McpApisPublicasBr.Cep;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = Host.CreateApplicationBuilder(args);

builder.Logging.AddConsole(options =>
{
    options.LogToStandardErrorThreshold = LogLevel.Trace;
});

builder.Services.AddHttpClient<ConsultaCep>(cliente =>
{
    cliente.BaseAddress = new Uri("https://viacep.com.br/");
    cliente.Timeout = TimeSpan.FromSeconds(5);
});

builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithTools<CepFerramenta>();

await builder.Build().RunAsync();
