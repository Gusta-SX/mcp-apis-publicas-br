# APIs públicas brasileiras no Cursor

Servidor MCP em C# para consultar CEP, CNPJ, FIPE, Banco Central e IBGE a partir do Cursor, sem copiar a resposta de um site.

Nesta versão o processo sobe, fala pelo stdio e ainda não expõe ferramenta. As cinco consultas entram uma por vez.

## Ferramentas previstas

- CEP. Uma consulta, tempo limite e erro claro quando o CEP não existe.
- CNPJ. Nome, situação e endereço. O restante do cadastro fica de fora.
- FIPE. Marca, modelo e ano em ferramentas pequenas.
- Banco Central. Uma série, com data inicial, data final e teto de intervalo.
- IBGE. Localidade, no mesmo cliente HTTP das outras.

## O que esta versão não faz

- Não consulta SQL Server.
- Não pede chave paga e não hospeda um servidor na internet.
- Não publica pacote no NuGet.
- Não inclui o assistente de anúncios.

## Como compilar

```bash
dotnet build mcp-apis-publicas-br.sln
```

O `global.json` pede o SDK .NET 8.

O cliente MCP inicia o processo e fala por stdin/stdout. Log vai para o stderr, para não misturar com o protocolo. Rodar `dotnet run` no terminal deixa o processo esperando essa conversa. Ainda não há ferramenta para chamar.
