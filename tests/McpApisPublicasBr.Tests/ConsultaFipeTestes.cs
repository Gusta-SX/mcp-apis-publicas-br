using System.Net;
using McpApisPublicasBr.Fipe;

namespace McpApisPublicasBr.Tests;

public class ConsultaFipeTestes
{
    [Fact]
    public async Task Marcas_devolvem_codigo_e_nome()
    {
        var consulta = Criar(HttpStatusCode.OK, """
            [{"codigo":"21","nome":"Fiat"},{"codigo":"59","nome":"VW - VolksWagen"}]
            """);

        var texto = await consulta.ListarMarcasAsync("Carros");

        Assert.Equal("21: Fiat\n59: VW - VolksWagen", texto);
    }

    [Fact]
    public async Task Modelos_ignoram_os_anos_da_marca()
    {
        var consulta = Criar(HttpStatusCode.OK, """
            {
              "modelos": [{ "codigo": 4828, "nome": "Palio 1.0 ECONOMY Fire Flex 8V 4p" }],
              "anos": [{ "codigo": "2014-5", "nome": "ANO QUE NAO PODE APARECER" }]
            }
            """);

        var texto = await consulta.ListarModelosAsync("carros", "21");

        Assert.Equal("4828: Palio 1.0 ECONOMY Fire Flex 8V 4p", texto);
        Assert.DoesNotContain("ANO QUE NAO PODE APARECER", texto);
    }

    [Fact]
    public async Task Anos_devolvem_codigo_e_nome()
    {
        var consulta = Criar(HttpStatusCode.OK, """
            [{"codigo":"2014-5","nome":"2014 Flex"}]
            """);

        var texto = await consulta.ListarAnosAsync("carros", "21", "4828");

        Assert.Equal("2014-5: 2014 Flex", texto);
    }

    [Fact]
    public async Task Preco_devolve_so_a_linha_curta()
    {
        var consulta = Criar(HttpStatusCode.OK, """
            {
              "TipoVeiculo": 1,
              "Valor": "R$ 32.553,00",
              "Marca": "Fiat",
              "Modelo": "Palio 1.0 ECONOMY Fire Flex 8V 4p",
              "AnoModelo": 2014,
              "Combustivel": "Flex",
              "CodigoFipe": "001267-0",
              "MesReferencia": "novembro de 2026 ",
              "SiglaCombustivel": "F"
            }
            """);

        var texto = await consulta.ConsultarPrecoAsync("carros", "21", "4828", "2014-5");

        Assert.Equal(
            "Fiat Palio 1.0 ECONOMY Fire Flex 8V 4p, 2014 Flex. Valor: R$ 32.553,00. Código FIPE: 001267-0. Referência: novembro de 2026",
            texto);
    }

    [Fact]
    public async Task Tipo_invalido_nao_consulta_a_rede()
    {
        var consulta = Criar(HttpStatusCode.OK, "[]", falhaSeChamar: true);

        var texto = await consulta.ListarMarcasAsync("bicicleta");

        Assert.Equal(ConsultaFipe.TipoInvalido, texto);
    }

    [Fact]
    public async Task Ano_inexistente_diz_que_nao_encontrou()
    {
        var consulta = Criar(HttpStatusCode.NotFound, """{"error":"veículo não encontrado"}""");

        var texto = await consulta.ConsultarPrecoAsync("carros", "21", "4828", "2014-1");

        Assert.Equal(ConsultaFipe.AnoNaoEncontrado, texto);
    }

    private static ConsultaFipe Criar(HttpStatusCode status, string json, bool falhaSeChamar = false)
    {
        var handler = new GravadoHandler(status, json, falhaSeChamar);
        var cliente = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://parallelum.com.br/"),
            Timeout = TimeSpan.FromSeconds(5)
        };
        return new ConsultaFipe(cliente);
    }

    private sealed class GravadoHandler(HttpStatusCode status, string json, bool falhaSeChamar) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (falhaSeChamar)
            {
                throw new InvalidOperationException("A consulta saiu da máquina.");
            }

            return Task.FromResult(new HttpResponseMessage(status)
            {
                Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json")
            });
        }
    }
}
