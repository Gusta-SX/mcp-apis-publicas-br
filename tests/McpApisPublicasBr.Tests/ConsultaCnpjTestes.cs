using System.Net;
using McpApisPublicasBr.Cnpj;

namespace McpApisPublicasBr.Tests;

public class ConsultaCnpjTestes
{
    private const string CadastroGravado = """
        {
          "cnpj": "00000000000191",
          "razao_social": "BANCO DO BRASIL SA",
          "nome_fantasia": "DIRECAO GERAL",
          "descricao_situacao_cadastral": "ATIVA",
          "logradouro": "SAUN QUADRA 5 BLOCO B TORRE I, II, III",
          "numero": "SN",
          "complemento": "ANDAR T I",
          "bairro": "ASA NORTE",
          "municipio": "BRASILIA",
          "uf": "DF",
          "cep": "70040912",
          "capital_social": 120000000000,
          "cnae_fiscal_descricao": "Bancos múltiplos, com carteira comercial",
          "qsa": [{ "nome_socio": "SOCIO QUE NAO PODE APARECER", "cnpj_cpf_do_socio": "***000000**" }]
        }
        """;

    [Fact]
    public async Task Encontrado_devolve_nome_situacao_e_endereco()
    {
        var consulta = Criar(HttpStatusCode.OK, CadastroGravado);

        var texto = await consulta.ConsultarAsync("00.000.000/0001-91");

        Assert.Equal(
            "00.000.000/0001-91: BANCO DO BRASIL SA. Situação: ATIVA. Endereço: SAUN QUADRA 5 BLOCO B TORRE I, II, III, SN, ASA NORTE, BRASILIA/DF",
            texto);
        Assert.DoesNotContain("SOCIO QUE NAO PODE APARECER", texto);
        Assert.DoesNotContain("DIRECAO GERAL", texto);
        Assert.DoesNotContain("Bancos múltiplos", texto);
    }

    [Fact]
    public async Task Inexistente_diz_que_nao_encontrou()
    {
        var consulta = Criar(HttpStatusCode.NotFound, """{"name":"NotFoundError"}""");

        var texto = await consulta.ConsultarAsync("99999999000191");

        Assert.Equal(ConsultaCnpj.NaoEncontrado, texto);
    }

    [Fact]
    public async Task Digitos_invalidos_diz_que_o_formato_e_invalido()
    {
        var consulta = Criar(HttpStatusCode.BadRequest, """{"name":"BadRequestError"}""");

        var texto = await consulta.ConsultarAsync("00000000000190");

        Assert.Equal(ConsultaCnpj.FormatoInvalido, texto);
    }

    [Fact]
    public async Task Formato_invalido_nao_consulta_a_rede()
    {
        var consulta = Criar(HttpStatusCode.OK, CadastroGravado, falhaSeChamar: true);

        var texto = await consulta.ConsultarAsync("123");

        Assert.Equal(ConsultaCnpj.FormatoInvalido, texto);
    }

    private static ConsultaCnpj Criar(HttpStatusCode status, string json, bool falhaSeChamar = false)
    {
        var handler = new GravadoHandler(status, json, falhaSeChamar);
        var cliente = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://brasilapi.com.br/"),
            Timeout = TimeSpan.FromSeconds(5)
        };
        return new ConsultaCnpj(cliente);
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
