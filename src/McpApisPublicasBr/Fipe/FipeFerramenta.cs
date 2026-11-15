using System.ComponentModel;
using ModelContextProtocol.Server;

namespace McpApisPublicasBr.Fipe;

[McpServerToolType]
public sealed class FipeFerramenta(ConsultaFipe consulta)
{
    [McpServerTool(Name = "consultar_marcas_fipe")]
    [Description("Lista as marcas da tabela FIPE para carros, motos ou caminhoes. Devolve só código e nome.")]
    public Task<string> Marcas(
        [Description("Tipo do veículo: carros, motos ou caminhoes.")] string tipo,
        CancellationToken cancellationToken)
    {
        return consulta.ListarMarcasAsync(tipo, cancellationToken);
    }

    [McpServerTool(Name = "consultar_modelos_fipe")]
    [Description("Lista os modelos de uma marca na FIPE. Devolve só código e nome.")]
    public Task<string> Modelos(
        [Description("Tipo do veículo: carros, motos ou caminhoes.")] string tipo,
        [Description("Código da marca, vindo da lista de marcas.")] string codigoMarca,
        CancellationToken cancellationToken)
    {
        return consulta.ListarModelosAsync(tipo, codigoMarca, cancellationToken);
    }

    [McpServerTool(Name = "consultar_anos_fipe")]
    [Description("Lista os anos de um modelo na FIPE. Devolve só código e nome.")]
    public Task<string> Anos(
        [Description("Tipo do veículo: carros, motos ou caminhoes.")] string tipo,
        [Description("Código da marca.")] string codigoMarca,
        [Description("Código do modelo, vindo da lista de modelos.")] string codigoModelo,
        CancellationToken cancellationToken)
    {
        return consulta.ListarAnosAsync(tipo, codigoMarca, codigoModelo, cancellationToken);
    }

    [McpServerTool(Name = "consultar_preco_fipe")]
    [Description("Consulta o preço de um ano na FIPE. Devolve marca, modelo, ano, valor, código FIPE e mês de referência.")]
    public Task<string> Preco(
        [Description("Tipo do veículo: carros, motos ou caminhoes.")] string tipo,
        [Description("Código da marca.")] string codigoMarca,
        [Description("Código do modelo.")] string codigoModelo,
        [Description("Código do ano, vindo da lista de anos, como 2014-5.")] string codigoAno,
        CancellationToken cancellationToken)
    {
        return consulta.ConsultarPrecoAsync(tipo, codigoMarca, codigoModelo, codigoAno, cancellationToken);
    }
}
