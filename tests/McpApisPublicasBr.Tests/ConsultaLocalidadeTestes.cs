using System.Net;
using McpApisPublicasBr.Ibge;

namespace McpApisPublicasBr.Tests;

public class ConsultaLocalidadeTestes
{
    private const string SaoPauloGravado = """
        {
          "id": 3550308,
          "nome": "São Paulo",
          "microrregiao": {
            "id": 35061,
            "nome": "São Paulo",
            "mesorregiao": {
              "id": 3515,
              "nome": "Metropolitana de São Paulo",
              "UF": {
                "id": 35,
                "sigla": "SP",
                "nome": "São Paulo",
                "regiao": { "id": 3, "sigla": "SE", "nome": "Sudeste" }
              }
            }
          },
          "regiao-imediata": {
            "id": 350001,
            "nome": "São Paulo",
            "regiao-intermediaria": {
              "id": 3501,
              "nome": "São Paulo",
              "UF": {
                "id": 35,
                "sigla": "SP",
                "nome": "São Paulo",
                "regiao": { "id": 3, "sigla": "SE", "nome": "Sudeste" }
              }
            }
          }
        }
        """;

    [Fact]
    public async Task Encontrada_devolve_nome_uf_e_regiao()
    {
        var consulta = Criar(HttpStatusCode.OK, SaoPauloGravado);

        var texto = await consulta.ConsultarAsync("3550308");

        Assert.Equal("3550308: São Paulo, São Paulo/SP, Sudeste", texto);
        Assert.DoesNotContain("Metropolitana", texto);
        Assert.DoesNotContain("35061", texto);
    }

    [Fact]
    public async Task Sem_microrregiao_usa_a_regiao_imediata()
    {
        var consulta = Criar(HttpStatusCode.OK, """
            {
              "id": 2605459,
              "nome": "Fernando de Noronha",
              "microrregiao": null,
              "regiao-imediata": {
                "nome": "Recife",
                "regiao-intermediaria": {
                  "nome": "Recife",
                  "UF": {
                    "sigla": "PE",
                    "nome": "Pernambuco",
                    "regiao": { "nome": "Nordeste" }
                  }
                }
              }
            }
            """);

        var texto = await consulta.ConsultarAsync("2605459");

        Assert.Equal("2605459: Fernando de Noronha, Pernambuco/PE, Nordeste", texto);
        Assert.DoesNotContain("Recife", texto);
    }

    [Fact]
    public async Task Lista_vazia_diz_que_nao_encontrou()
    {
        var consulta = Criar(HttpStatusCode.OK, "[]");

        var texto = await consulta.ConsultarAsync("0000000");

        Assert.Equal(ConsultaLocalidade.NaoEncontrada, texto);
    }

    [Fact]
    public async Task Codigo_invalido_nao_consulta_a_rede()
    {
        var consulta = Criar(HttpStatusCode.OK, SaoPauloGravado, falhaSeChamar: true);

        var texto = await consulta.ConsultarAsync("123");

        Assert.Equal(ConsultaLocalidade.CodigoInvalido, texto);
    }

    private static ConsultaLocalidade Criar(HttpStatusCode status, string json, bool falhaSeChamar = false)
    {
        var handler = new GravadoHandler(status, json, falhaSeChamar);
        var cliente = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://servicodados.ibge.gov.br/"),
            Timeout = TimeSpan.FromSeconds(5)
        };
        return new ConsultaLocalidade(cliente);
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
