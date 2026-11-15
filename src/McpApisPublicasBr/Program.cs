using McpApisPublicasBr.BancoCentral;
using McpApisPublicasBr.Cep;
using McpApisPublicasBr.Cnpj;
using McpApisPublicasBr.Fipe;
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

builder.Services.AddHttpClient<ConsultaCnpj>(cliente =>
{
    cliente.BaseAddress = new Uri("https://brasilapi.com.br/");
    cliente.Timeout = TimeSpan.FromSeconds(5);
});

builder.Services.AddHttpClient<ConsultaFipe>(cliente =>
{
    cliente.BaseAddress = new Uri("https://parallelum.com.br/");
    cliente.Timeout = TimeSpan.FromSeconds(5);
});

builder.Services.AddHttpClient<ConsultaSerie>(cliente =>
{
    cliente.BaseAddress = new Uri("https://api.bcb.gov.br/");
    cliente.Timeout = TimeSpan.FromSeconds(5);
});

builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithTools<CepFerramenta>()
    .WithTools<CnpjFerramenta>()
    .WithTools<FipeFerramenta>()
    .WithTools<SerieFerramenta>();

await builder.Build().RunAsync();
