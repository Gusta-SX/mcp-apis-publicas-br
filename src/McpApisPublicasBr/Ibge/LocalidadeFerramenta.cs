using System.ComponentModel;
using ModelContextProtocol.Server;

namespace McpApisPublicasBr.Ibge;

[McpServerToolType]
public sealed class LocalidadeFerramenta(ConsultaLocalidade consulta)
{
    [McpServerTool(Name = "consultar_localidade_ibge")]
    [Description("Consulta um município pelo código do IBGE e devolve nome, UF e região. Não inclui o restante da divisão territorial.")]
    public Task<string> Consultar(
        [Description("Código de 7 dígitos do município no IBGE.")] string codigo,
        CancellationToken cancellationToken)
    {
        return consulta.ConsultarAsync(codigo, cancellationToken);
    }
}
