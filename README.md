# APIs públicas brasileiras no Cursor

Servidor MCP em C# para consultar dados públicos a partir do Cursor, sem copiar a resposta de um site.

As ferramentas prontas são CEP, CNPJ, FIPE, a série do Banco Central e a localidade do IBGE.

## CEP

A ferramenta se chama `consultar_cep`. Ela aceita o CEP com ou sem hífen e devolve só cinco campos: CEP, logradouro, bairro, cidade e UF.

- Um CEP que não tem 8 dígitos não sai da máquina. A resposta diz que o formato é inválido.
- A chamada ao ViaCEP tem tempo limite de 5 segundos. Não usa chave.
- CEP inexistente vira a frase “CEP não encontrado”.
- Falha de rede ou tempo esgotado vira uma frase clara, sem o JSON cru.

## CNPJ

A ferramenta se chama `consultar_cnpj`. Ela aceita o CNPJ com ou sem pontuação e devolve só três dados: razão social, situação e endereço.

- Um CNPJ que não tem 14 dígitos não sai da máquina. A resposta diz que o formato é inválido.
- A chamada à BrasilAPI tem tempo limite de 5 segundos. Não usa chave.
- CNPJ inexistente vira a frase “CNPJ não encontrado”.
- Sócios, nome fantasia, CNAE, capital e o restante do cadastro ficam de fora.
- Falha de rede ou tempo esgotado vira uma frase clara, sem o JSON cru.

## FIPE

Quatro ferramentas pequenas, na ordem da consulta. O tipo é `carros`, `motos` ou `caminhoes`. Não usa chave e cada chamada tem tempo limite de 5 segundos.

- `consultar_marcas_fipe` devolve código e nome da marca.
- `consultar_modelos_fipe` recebe o código da marca e devolve código e nome do modelo.
- `consultar_anos_fipe` recebe marca e modelo e devolve o código do ano, como `2014-5`.
- `consultar_preco_fipe` devolve marca, modelo, ano, valor, código FIPE e mês de referência.

Um tipo fora dessa lista não sai da máquina. Marca, modelo ou ano inexistente vira uma frase clara. Falha de rede ou tempo esgotado também, sem o JSON cru.

## Banco Central

A ferramenta se chama `consultar_serie_bcb`. Ela recebe o número da série do SGS, a data inicial e a data final, e devolve só data e valor.

- A data entra como `dd/MM/aaaa` ou `aaaa-MM-dd`.
- O intervalo máximo é de 90 dias. Acima disso, a consulta não sai da máquina.
- A série 1 é o dólar de venda.
- Série inexistente vira a frase “Série não encontrada”.
- A chamada tem tempo limite de 5 segundos e não usa chave.
- Falha de rede ou tempo esgotado vira uma frase clara, sem o texto cru da API.

## IBGE

A ferramenta se chama `consultar_localidade_ibge`. Ela recebe o código de 7 dígitos do município e devolve nome, UF e região.

- Um código que não tem 7 dígitos não sai da máquina.
- A chamada ao IBGE tem tempo limite de 5 segundos. Não usa chave.
- Município inexistente vira a frase “Localidade não encontrada”.
- Microrregião, mesorregião e região imediata ficam de fora.
- Falha de rede ou tempo esgotado vira uma frase clara, sem o JSON cru.

## O que esta versão não faz

- Não consulta SQL Server.
- Não pede chave paga e não hospeda um servidor na internet.
- Não publica pacote no NuGet.
- Não inclui o assistente de anúncios.

## Como compilar e testar

```bash
dotnet test mcp-apis-publicas-br.sln
```

O `global.json` pede o SDK .NET 8. Os testes usam uma resposta gravada e não acessam a internet.

O cliente MCP inicia o processo e fala por stdin/stdout. Log vai para o stderr, para não misturar com o protocolo. Rodar `dotnet run` no terminal deixa o processo esperando essa conversa.
