using System.ComponentModel;
using ModelContextProtocol.Server;

namespace McpApisPublicasBr.BancoCentral;

[McpServerToolType]
public sealed class SerieFerramenta(ConsultaSerie consulta)
{
    [McpServerTool(Name = "consultar_serie_bcb")]
    [Description("Consulta uma série do SGS do Banco Central entre duas datas. O intervalo máximo é de 90 dias. Devolve data e valor.")]
    public Task<string> Consultar(
        [Description("Número da série no SGS. A série 1 é o dólar de venda.")] string codigo,
        [Description("Data inicial, em dd/MM/aaaa ou aaaa-MM-dd.")] string dataInicial,
        [Description("Data final, em dd/MM/aaaa ou aaaa-MM-dd.")] string dataFinal,
        CancellationToken cancellationToken)
    {
        return consulta.ConsultarAsync(codigo, dataInicial, dataFinal, cancellationToken);
    }
}
