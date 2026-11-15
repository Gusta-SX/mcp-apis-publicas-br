using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace McpApisPublicasBr.BancoCentral;

public sealed class ConsultaSerie(HttpClient http)
{
    public const int IntervaloMaximoEmDias = 90;
    public const string CodigoInvalido = "Código da série inválido. Use o número da série do SGS.";
    public const string DataInvalida = "Data inválida. Use dd/MM/aaaa ou aaaa-MM-dd.";
    public const string DataInvertida = "A data final é anterior à data inicial.";
    public const string IntervaloLongo = "O intervalo máximo é de 90 dias.";
    public const string NaoEncontrada = "Série não encontrada.";
    public const string SemDados = "Série sem dados nesse intervalo.";
    public const string Indisponivel = "Não foi possível consultar o Banco Central agora.";
    public const string TempoEsgotado = "A consulta do Banco Central excedeu o tempo limite.";

    public async Task<string> ConsultarAsync(string codigo, string dataInicial, string dataFinal, CancellationToken cancellationToken = default)
    {
        var serie = codigo.Trim();
        if (serie.Length is 0 or > 6 || !serie.All(char.IsDigit))
        {
            return CodigoInvalido;
        }

        if (!LerData(dataInicial, out var inicio) || !LerData(dataFinal, out var fim))
        {
            return DataInvalida;
        }

        var dias = (fim - inicio).Days;
        if (dias < 0)
        {
            return DataInvertida;
        }

        if (dias > IntervaloMaximoEmDias - 1)
        {
            return IntervaloLongo;
        }

        var inicioTexto = inicio.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
        var fimTexto = fim.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
        var caminho = $"dados/serie/bcdata.sgs.{serie}/dados?formato=json&dataInicial={Uri.EscapeDataString(inicioTexto)}&dataFinal={Uri.EscapeDataString(fimTexto)}";

        try
        {
            using var resposta = await http.GetAsync(caminho, cancellationToken);
            if (resposta.StatusCode == HttpStatusCode.NotFound)
            {
                return NaoEncontrada;
            }

            if (!resposta.IsSuccessStatusCode)
            {
                return Indisponivel;
            }

            var pontos = await resposta.Content.ReadFromJsonAsync<List<Ponto>>(cancellationToken);
            if (pontos is null || pontos.Count == 0)
            {
                return SemDados;
            }

            return string.Join('\n', pontos.Select(ponto => $"{ponto.Data}: {ponto.Valor}"));
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return TempoEsgotado;
        }
        catch (HttpRequestException)
        {
            return Indisponivel;
        }
    }

    private static bool LerData(string texto, out DateTime data)
    {
        return DateTime.TryParseExact(
            texto.Trim(),
            ["dd/MM/yyyy", "yyyy-MM-dd"],
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out data);
    }

    private sealed class Ponto
    {
        [JsonPropertyName("data")]
        public string Data { get; set; } = "";

        [JsonPropertyName("valor")]
        public string Valor { get; set; } = "";
    }
}
