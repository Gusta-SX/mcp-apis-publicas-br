using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace McpApisPublicasBr.Fipe;

public sealed class ConsultaFipe(HttpClient http)
{
    public const string TipoInvalido = "Tipo inválido. Use carros, motos ou caminhoes.";
    public const string CodigoMarcaInvalido = "Código de marca inválido. Use o código da lista de marcas.";
    public const string CodigoModeloInvalido = "Código de modelo inválido. Use o código da lista de modelos.";
    public const string CodigoAnoInvalido = "Código de ano inválido. Use o código da lista, como 2014-5.";
    public const string MarcaNaoEncontrada = "Marca não encontrada.";
    public const string ModeloNaoEncontrado = "Modelo não encontrado.";
    public const string AnoNaoEncontrado = "Ano não encontrado.";
    public const string NenhumaMarca = "Nenhuma marca encontrada.";
    public const string NenhumModelo = "Nenhum modelo encontrado.";
    public const string NenhumAno = "Nenhum ano encontrado.";
    public const string Indisponivel = "Não foi possível consultar a FIPE agora.";
    public const string TempoEsgotado = "A consulta da FIPE excedeu o tempo limite.";

    public Task<string> ListarMarcasAsync(string tipo, CancellationToken cancellationToken = default)
    {
        if (!NormalizarTipo(tipo, out var tipoNormalizado))
        {
            return Task.FromResult(TipoInvalido);
        }

        return ConsultarAsync(
            $"fipe/api/v1/{tipoNormalizado}/marcas",
            MarcaNaoEncontrada,
            async (conteudo, token) =>
            {
                var marcas = await conteudo.ReadFromJsonAsync<List<Item>>(token);
                return Linhas(marcas, NenhumaMarca);
            },
            cancellationToken);
    }

    public Task<string> ListarModelosAsync(string tipo, string codigoMarca, CancellationToken cancellationToken = default)
    {
        if (!NormalizarTipo(tipo, out var tipoNormalizado))
        {
            return Task.FromResult(TipoInvalido);
        }

        if (!CodigoNumerico(codigoMarca, out var marca))
        {
            return Task.FromResult(CodigoMarcaInvalido);
        }

        return ConsultarAsync(
            $"fipe/api/v1/{tipoNormalizado}/marcas/{marca}/modelos",
            MarcaNaoEncontrada,
            async (conteudo, token) =>
            {
                var corpo = await conteudo.ReadFromJsonAsync<RespostaModelos>(token);
                return Linhas(corpo?.Modelos, NenhumModelo);
            },
            cancellationToken);
    }

    public Task<string> ListarAnosAsync(string tipo, string codigoMarca, string codigoModelo, CancellationToken cancellationToken = default)
    {
        if (!NormalizarTipo(tipo, out var tipoNormalizado))
        {
            return Task.FromResult(TipoInvalido);
        }

        if (!CodigoNumerico(codigoMarca, out var marca))
        {
            return Task.FromResult(CodigoMarcaInvalido);
        }

        if (!CodigoNumerico(codigoModelo, out var modelo))
        {
            return Task.FromResult(CodigoModeloInvalido);
        }

        return ConsultarAsync(
            $"fipe/api/v1/{tipoNormalizado}/marcas/{marca}/modelos/{modelo}/anos",
            ModeloNaoEncontrado,
            async (conteudo, token) =>
            {
                var anos = await conteudo.ReadFromJsonAsync<List<Item>>(token);
                return Linhas(anos, NenhumAno);
            },
            cancellationToken);
    }

    public Task<string> ConsultarPrecoAsync(string tipo, string codigoMarca, string codigoModelo, string codigoAno, CancellationToken cancellationToken = default)
    {
        if (!NormalizarTipo(tipo, out var tipoNormalizado))
        {
            return Task.FromResult(TipoInvalido);
        }

        if (!CodigoNumerico(codigoMarca, out var marca))
        {
            return Task.FromResult(CodigoMarcaInvalido);
        }

        if (!CodigoNumerico(codigoModelo, out var modelo))
        {
            return Task.FromResult(CodigoModeloInvalido);
        }

        var ano = codigoAno.Trim();
        var hifen = ano.IndexOf('-');
        var anoValido = hifen is 4 or 5
            && ano.Length == hifen + 2
            && ano[..hifen].All(char.IsDigit)
            && char.IsDigit(ano[^1]);
        if (!anoValido)
        {
            return Task.FromResult(CodigoAnoInvalido);
        }

        return ConsultarAsync(
            $"fipe/api/v1/{tipoNormalizado}/marcas/{marca}/modelos/{modelo}/anos/{ano}",
            AnoNaoEncontrado,
            async (conteudo, token) =>
            {
                var corpo = await conteudo.ReadFromJsonAsync<RespostaPreco>(token);
                if (corpo is null || string.IsNullOrWhiteSpace(corpo.Valor))
                {
                    return AnoNaoEncontrado;
                }

                return $"{corpo.Marca} {corpo.Modelo}, {corpo.AnoModelo} {corpo.Combustivel}. Valor: {corpo.Valor}. Código FIPE: {corpo.CodigoFipe}. Referência: {corpo.MesReferencia.Trim()}";
            },
            cancellationToken);
    }

    private async Task<string> ConsultarAsync(
        string caminho,
        string naoEncontrado,
        Func<HttpContent, CancellationToken, Task<string>> ler,
        CancellationToken cancellationToken)
    {
        try
        {
            using var resposta = await http.GetAsync(caminho, cancellationToken);
            if (resposta.StatusCode == HttpStatusCode.NotFound)
            {
                return naoEncontrado;
            }

            if (!resposta.IsSuccessStatusCode)
            {
                return Indisponivel;
            }

            return await ler(resposta.Content, cancellationToken);
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

    private static bool NormalizarTipo(string tipo, out string normalizado)
    {
        normalizado = tipo.Trim().ToLowerInvariant();
        return normalizado is "carros" or "motos" or "caminhoes";
    }

    private static bool CodigoNumerico(string codigo, out string digitos)
    {
        digitos = codigo.Trim();
        return digitos.Length > 0 && digitos.All(char.IsDigit);
    }

    private static string Linhas(IReadOnlyList<Item>? itens, string vazio)
    {
        if (itens is null || itens.Count == 0)
        {
            return vazio;
        }

        return string.Join('\n', itens.Select(item => $"{item.CodigoTexto}: {item.Nome}"));
    }

    private sealed class RespostaModelos
    {
        [JsonPropertyName("modelos")]
        public List<Item> Modelos { get; set; } = [];
    }

    private sealed class Item
    {
        [JsonPropertyName("codigo")]
        public JsonElement Codigo { get; set; }

        [JsonPropertyName("nome")]
        public string Nome { get; set; } = "";

        public string CodigoTexto => Codigo.ValueKind switch
        {
            JsonValueKind.String => Codigo.GetString() ?? "",
            JsonValueKind.Number => Codigo.GetRawText(),
            _ => ""
        };
    }

    private sealed class RespostaPreco
    {
        [JsonPropertyName("Valor")]
        public string Valor { get; set; } = "";

        [JsonPropertyName("Marca")]
        public string Marca { get; set; } = "";

        [JsonPropertyName("Modelo")]
        public string Modelo { get; set; } = "";

        [JsonPropertyName("AnoModelo")]
        public int AnoModelo { get; set; }

        [JsonPropertyName("Combustivel")]
        public string Combustivel { get; set; } = "";

        [JsonPropertyName("CodigoFipe")]
        public string CodigoFipe { get; set; } = "";

        [JsonPropertyName("MesReferencia")]
        public string MesReferencia { get; set; } = "";
    }
}
