using System.ComponentModel;
using ModelContextProtocol.Server;

namespace McpApisPublicasBr.Cnpj;

[McpServerToolType]
public sealed class CnpjFerramenta(ConsultaCnpj consulta)
{
    [McpServerTool(Name = "consultar_cnpj")]
    [Description("Consulta um CNPJ brasileiro e devolve razão social, situação e endereço. Não inclui sócios nem o restante do cadastro.")]
    public Task<string> Consultar(string cnpj, CancellationToken cancellationToken)
    {
        return consulta.ConsultarAsync(cnpj, cancellationToken);
    }
}
