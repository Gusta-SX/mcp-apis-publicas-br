using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace McpApisPublicasBr.Cep;

public sealed class ConsultaCep(HttpClient http)
{
    public const string FormatoInvalido = "CEP inválido. Use 8 dígitos, com ou sem hífen.";
    public const string NaoEncontrado = "CEP não encontrado.";
    public const string Indisponivel = "Não foi possível consultar o CEP agora.";
    public const string TempoEsgotado = "A consulta de CEP excedeu o tempo limite.";

    public async Task<string> ConsultarAsync(string cep, CancellationToken cancellationToken = default)
    {
        var digitos = new string(cep.Where(char.IsDigit).ToArray());
        if (digitos.Length != 8)
        {
            return FormatoInvalido;
        }

        try
        {
            using var resposta = await http.GetAsync($"ws/{digitos}/json/", cancellationToken);
            if (!resposta.IsSuccessStatusCode)
            {
                return Indisponivel;
            }

            var corpo = await resposta.Content.ReadFromJsonAsync<RespostaViaCep>(cancellationToken);
            if (corpo is null || corpo.Erro)
            {
                return NaoEncontrado;
            }

            return $"{corpo.Cep}: {corpo.Logradouro}, {corpo.Bairro}, {corpo.Localidade}/{corpo.Uf}";
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

    private sealed class RespostaViaCep
    {
        [JsonPropertyName("cep")]
        public string Cep { get; set; } = "";

        [JsonPropertyName("logradouro")]
        public string Logradouro { get; set; } = "";

        [JsonPropertyName("bairro")]
        public string Bairro { get; set; } = "";

        [JsonPropertyName("localidade")]
        public string Localidade { get; set; } = "";

        [JsonPropertyName("uf")]
        public string Uf { get; set; } = "";

        [JsonPropertyName("erro")]
        public bool Erro { get; set; }
    }
}
