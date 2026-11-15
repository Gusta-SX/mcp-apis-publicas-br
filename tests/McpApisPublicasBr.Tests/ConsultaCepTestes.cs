using System.Net;
using McpApisPublicasBr.Cep;

namespace McpApisPublicasBr.Tests;

public class ConsultaCepTestes
{
    private const string EnderecoGravado = """
        {
          "cep": "01001-000",
          "logradouro": "Praça da Sé",
          "complemento": "lado ímpar",
          "unidade": "",
          "bairro": "Sé",
          "localidade": "São Paulo",
          "uf": "SP",
          "estado": "São Paulo",
          "regiao": "Sudeste",
          "ibge": "3550308",
          "gia": "1004",
          "ddd": "11",
          "siafi": "7107"
        }
        """;

    [Fact]
    public async Task Encontrado_devolve_so_o_endereco_curto()
    {
        var consulta = Criar(HttpStatusCode.OK, EnderecoGravado);

        var texto = await consulta.ConsultarAsync("01001-000");

        Assert.Equal("01001-000: Praça da Sé, Sé, São Paulo/SP", texto);
    }

    [Fact]
    public async Task Inexistente_diz_que_nao_encontrou()
    {
        var consulta = Criar(HttpStatusCode.OK, """{"erro": true}""");

        var texto = await consulta.ConsultarAsync("00000000");

        Assert.Equal(ConsultaCep.NaoEncontrado, texto);
    }

    [Fact]
    public async Task Formato_invalido_nao_consulta_a_rede()
    {
        var consulta = Criar(HttpStatusCode.OK, EnderecoGravado, falhaSeChamar: true);

        var texto = await consulta.ConsultarAsync("123");

        Assert.Equal(ConsultaCep.FormatoInvalido, texto);
    }

    private static ConsultaCep Criar(HttpStatusCode status, string json, bool falhaSeChamar = false)
    {
        var handler = new GravadoHandler(status, json, falhaSeChamar);
        var cliente = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://viacep.com.br/"),
            Timeout = TimeSpan.FromSeconds(5)
        };
        return new ConsultaCep(cliente);
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
