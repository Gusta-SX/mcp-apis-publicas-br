# APIs públicas brasileiras no Cursor

Servidor MCP em C# para consultar dados públicos a partir do Cursor, sem copiar a resposta de um site.

As ferramentas prontas são CEP e CNPJ. FIPE, Banco Central e IBGE entram depois, uma por vez.

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

## O que esta versão não faz

- Não consulta FIPE, Banco Central nem IBGE.
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
