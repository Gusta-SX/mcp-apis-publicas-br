using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace McpApisPublicasBr.Cnpj;

public sealed class ConsultaCnpj(HttpClient http)
{
    public const string FormatoInvalido = "CNPJ inválido. Use 14 dígitos, com ou sem pontuação.";
    public const string NaoEncontrado = "CNPJ não encontrado.";
    public const string Indisponivel = "Não foi possível consultar o CNPJ agora.";
    public const string TempoEsgotado = "A consulta de CNPJ excedeu o tempo limite.";

    public async Task<string> ConsultarAsync(string cnpj, CancellationToken cancellationToken = default)
    {
        var digitos = new string(cnpj.Where(char.IsDigit).ToArray());
        if (digitos.Length != 14)
        {
            return FormatoInvalido;
        }

        try
        {
            using var resposta = await http.GetAsync($"api/cnpj/v1/{digitos}", cancellationToken);
            if (resposta.StatusCode == HttpStatusCode.NotFound)
            {
                return NaoEncontrado;
            }

            if (resposta.StatusCode == HttpStatusCode.BadRequest)
            {
                return FormatoInvalido;
            }

            if (!resposta.IsSuccessStatusCode)
            {
                return Indisponivel;
            }

            var corpo = await resposta.Content.ReadFromJsonAsync<RespostaBrasilApi>(cancellationToken);
            if (corpo is null || string.IsNullOrWhiteSpace(corpo.RazaoSocial))
            {
                return NaoEncontrado;
            }

            var lugar = string.IsNullOrWhiteSpace(corpo.Municipio) && string.IsNullOrWhiteSpace(corpo.Uf)
                ? ""
                : $"{corpo.Municipio}/{corpo.Uf}";
            var endereco = string.Join(", ", new[]
            {
                corpo.Logradouro,
                corpo.Numero,
                corpo.Bairro,
                lugar
            }.Where(parte => !string.IsNullOrWhiteSpace(parte)));

            return $"{Formatar(digitos)}: {corpo.RazaoSocial}. Situação: {corpo.Situacao}. Endereço: {endereco}";
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

    private static string Formatar(string digitos)
    {
        return $"{digitos[..2]}.{digitos[2..5]}.{digitos[5..8]}/{digitos[8..12]}-{digitos[12..]}";
    }

    private sealed class RespostaBrasilApi
    {
        [JsonPropertyName("razao_social")]
        public string RazaoSocial { get; set; } = "";

        [JsonPropertyName("descricao_situacao_cadastral")]
        public string Situacao { get; set; } = "";

        [JsonPropertyName("logradouro")]
        public string Logradouro { get; set; } = "";

        [JsonPropertyName("numero")]
        public string Numero { get; set; } = "";

        [JsonPropertyName("bairro")]
        public string Bairro { get; set; } = "";

        [JsonPropertyName("municipio")]
        public string Municipio { get; set; } = "";

        [JsonPropertyName("uf")]
        public string Uf { get; set; } = "";
    }
}
