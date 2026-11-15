using System.Net;
using McpApisPublicasBr.BancoCentral;

namespace McpApisPublicasBr.Tests;

public class ConsultaSerieTestes
{
    private const string DolarGravado = """
        [
          {"data":"10/11/2026","valor":"5.5840"},
          {"data":"11/11/2026","valor":"5.5606"},
          {"data":"12/11/2026","valor":"5.6143"},
          {"data":"13/11/2026","valor":"5.6518"}
        ]
        """;

    [Fact]
    public async Task Encontrada_devolve_data_e_valor()
    {
        var consulta = Criar(HttpStatusCode.OK, DolarGravado);

        var texto = await consulta.ConsultarAsync("1", "2026-11-10", "14/11/2026");

        Assert.Equal("""
            10/11/2026: 5.5840
            11/11/2026: 5.5606
            12/11/2026: 5.6143
            13/11/2026: 5.6518
            """, texto);
    }

    [Fact]
    public async Task Sem_dados_diz_que_o_intervalo_esta_vazio()
    {
        var consulta = Criar(HttpStatusCode.OK, "[]");

        var texto = await consulta.ConsultarAsync("1", "01/01/2026", "31/03/2026");

        Assert.Equal(ConsultaSerie.SemDados, texto);
    }

    [Fact]
    public async Task Data_invalida_nao_consulta_a_rede()
    {
        var consulta = Criar(HttpStatusCode.OK, DolarGravado, falhaSeChamar: true);

        var texto = await consulta.ConsultarAsync("1", "ontem", "14/11/2026");

        Assert.Equal(ConsultaSerie.DataInvalida, texto);
    }

    [Fact]
    public async Task Intervalo_acima_de_90_dias_nao_consulta_a_rede()
    {
        var consulta = Criar(HttpStatusCode.OK, DolarGravado, falhaSeChamar: true);

        var texto = await consulta.ConsultarAsync("1", "01/01/2026", "01/04/2026");

        Assert.Equal(ConsultaSerie.IntervaloLongo, texto);
    }

    [Fact]
    public async Task Serie_inexistente_nao_devolve_o_texto_cru()
    {
        var consulta = Criar(HttpStatusCode.NotFound, "A serie nao existe!");

        var texto = await consulta.ConsultarAsync("999999", "10/11/2026", "14/11/2026");

        Assert.Equal(ConsultaSerie.NaoEncontrada, texto);
    }

    private static ConsultaSerie Criar(HttpStatusCode status, string corpo, bool falhaSeChamar = false)
    {
        var handler = new GravadoHandler(status, corpo, falhaSeChamar);
        var cliente = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://api.bcb.gov.br/"),
            Timeout = TimeSpan.FromSeconds(5)
        };
        return new ConsultaSerie(cliente);
    }

    private sealed class GravadoHandler(HttpStatusCode status, string corpo, bool falhaSeChamar) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (falhaSeChamar)
            {
                throw new InvalidOperationException("A consulta saiu da máquina.");
            }

            return Task.FromResult(new HttpResponseMessage(status)
            {
                Content = new StringContent(corpo)
            });
        }
    }
}
