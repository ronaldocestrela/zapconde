# language: pt-BR
Funcionalidade: Bootstrap da API SmartCondo com Minimal APIs e FastEndpoints
  Como desenvolvedor do sistema SmartCondo
  Eu quero que a API inicialize corretamente com FastEndpoints
  Para que possa servir endpoints funcionais com resposta padronizada Result<T>

  Contexto:
    Dado que a aplicação SmartCondo.Api está configurada para .NET 10
    E o template padrão WeatherForecast foi removido

  Cenário: A API inicializa com sucesso
    Quando a aplicação SmartCondo.Api é iniciada
    Então a aplicação deve subir sem erros
    E o host HTTP deve estar disponível

  Cenário: Endpoint de health responde com sucesso
    Dado que a API está em execução
    Quando eu faço uma requisição GET para "/api/health"
    Então o status HTTP deve ser 200 OK
    E a resposta deve conter a propriedade "isSuccess" com valor "true"

  Cenário: Resposta segue o padrão Result<T>
    Dado que a API está em execução
    Quando eu faço uma requisição GET para "/api/health"
    Então a resposta deve estar no formato JSON
    E deve conter as propriedades obrigatórias do envelope Result:
      | propriedade | tipo    |
      | isSuccess   | boolean |
      | message     | string  |
      | data        | object  |

  Cenário: FastEndpoints está configurado no pipeline
    Dado que a API está configurada
    Então o pipeline deve incluir middlewares do FastEndpoints
    E não deve conter referências ao endpoint WeatherForecast do template

  Cenário: A API responde rapidamente ao health check
    Dado que a API está em execução
    Quando eu faço uma requisição GET para "/api/health"
    Então a resposta deve ser retornada em menos de 100ms
    E o payload deve conter informações básicas de status do sistema
