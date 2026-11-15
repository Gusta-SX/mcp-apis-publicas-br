using System.ComponentModel;
using ModelContextProtocol.Server;

namespace McpApisPublicasBr.Cep;

[McpServerToolType]
public sealed class CepFerramenta(ConsultaCep consulta)
{
    [McpServerTool(Name = "consultar_cep")]
    [Description("Consulta um CEP brasileiro e devolve logradouro, bairro, cidade e UF. Não inclui o restante da ficha.")]
    public Task<string> Consultar(string cep, CancellationToken cancellationToken)
    {
        return consulta.ConsultarAsync(cep, cancellationToken);
    }
}
