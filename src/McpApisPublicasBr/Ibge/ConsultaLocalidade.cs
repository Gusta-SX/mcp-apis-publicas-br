using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace McpApisPublicasBr.Ibge;

public sealed class ConsultaLocalidade(HttpClient http)
{
    public const string CodigoInvalido = "Código inválido. Use os 7 dígitos do município no IBGE.";
    public const string NaoEncontrada = "Localidade não encontrada.";
    public const string Indisponivel = "Não foi possível consultar o IBGE agora.";
    public const string TempoEsgotado = "A consulta do IBGE excedeu o tempo limite.";

    public async Task<string> ConsultarAsync(string codigo, CancellationToken cancellationToken = default)
    {
        var digitos = codigo.Trim();
        if (digitos.Length != 7 || !digitos.All(char.IsDigit))
        {
            return CodigoInvalido;
        }

        try
        {
            using var resposta = await http.GetAsync($"api/v1/localidades/municipios/{digitos}", cancellationToken);
            if (resposta.StatusCode == HttpStatusCode.NotFound)
            {
                return NaoEncontrada;
            }

            if (!resposta.IsSuccessStatusCode)
            {
                return Indisponivel;
            }

            await using var fluxo = await resposta.Content.ReadAsStreamAsync(cancellationToken);
            using var json = await JsonDocument.ParseAsync(fluxo, cancellationToken: cancellationToken);
            if (json.RootElement.ValueKind != JsonValueKind.Object)
            {
                return NaoEncontrada;
            }

            var municipio = json.RootElement.Deserialize<Municipio>();
            if (municipio is null || string.IsNullOrWhiteSpace(municipio.Nome))
            {
                return NaoEncontrada;
            }

            var uf = municipio.Microrregiao?.Mesorregiao?.Uf
                ?? municipio.RegiaoImediata?.RegiaoIntermediaria?.Uf;
            if (uf is null || string.IsNullOrWhiteSpace(uf.Sigla))
            {
                return $"{digitos}: {municipio.Nome}";
            }

            return $"{digitos}: {municipio.Nome}, {uf.Nome}/{uf.Sigla}, {uf.Regiao?.Nome}";
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return TempoEsgotado;
        }
        catch (HttpRequestException)
        {
            return Indisponivel;
        }
        catch (JsonException)
        {
            return Indisponivel;
        }
    }

    private sealed class Municipio
    {
        [JsonPropertyName("nome")]
        public string Nome { get; set; } = "";

        [JsonPropertyName("microrregiao")]
        public Divisao? Microrregiao { get; set; }

        [JsonPropertyName("regiao-imediata")]
        public Divisao? RegiaoImediata { get; set; }
    }

    private sealed class Divisao
    {
        [JsonPropertyName("mesorregiao")]
        public Divisao? Mesorregiao { get; set; }

        [JsonPropertyName("regiao-intermediaria")]
        public Divisao? RegiaoIntermediaria { get; set; }

        [JsonPropertyName("UF")]
        public Uf? Uf { get; set; }
    }

    private sealed class Uf
    {
        [JsonPropertyName("sigla")]
        public string Sigla { get; set; } = "";

        [JsonPropertyName("nome")]
        public string Nome { get; set; } = "";

        [JsonPropertyName("regiao")]
        public Regiao? Regiao { get; set; }
    }

    private sealed class Regiao
    {
        [JsonPropertyName("nome")]
        public string Nome { get; set; } = "";
    }
}
