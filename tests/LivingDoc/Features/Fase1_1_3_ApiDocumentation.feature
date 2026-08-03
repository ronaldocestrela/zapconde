# language: pt-BR
Funcionalidade: Documentacao da API com OpenAPI e Scalar

  Como time tecnico da plataforma SmartCondo
  Quero disponibilizar documentacao da API em ambiente de desenvolvimento
  Para acelerar integracoes e manter o contrato vivo com base em XML Comments

  Cenario: Acessar documento OpenAPI da API
    Dado que a API SmartCondo esta em execucao no ambiente "Development"
    Quando eu acessar o endpoint "/openapi/v1.json"
    Entao a resposta deve ser "200 OK"
    E o payload deve conter o campo "openapi"

  Cenario: Acessar interface Scalar da API
    Dado que a API SmartCondo esta em execucao no ambiente "Development"
    Quando eu acessar o endpoint "/scalar"
    Entao a resposta deve ser "200 OK"

  Cenario: Exibir descricao de endpoint proveniente de XML Comments
    Dado que o endpoint de health possui XML Comments publicados
    Quando eu consultar o documento OpenAPI
    Entao a operacao "/api/health" deve conter resumo ou descricao gerada a partir dos XML Comments
