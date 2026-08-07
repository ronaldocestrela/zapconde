# language: pt-BR
Funcionalidade: Plugin AuthorizeGuest no Semantic Kernel para Liberacao de Visitantes na Portaria

  Como morador ou operador do condomínio
  Quero autorizar a entrada de visitantes e prestadores de serviço enviando instruções ao assistente de IA
  Para que a liberação na portaria seja registrada automaticamente com segurança e isolamento multi-tenant

  Cenário: Morador pré-autoriza visita de um amigo informando dados completos via IA
    Dado que o morador da unidade "102" no condomínio "1" está autenticado no sistema
    Quando o assistente de IA executa a ferramenta "AuthorizeGuest" com os seguintes parâmetros:
      | Campo         | Valor               |
      | nome          | Carlos Eduardo      |
      | documento     | 123.456.789-00      |
      | telefone      | +5575988887777      |
      | tipo          | Visitante           |
      | dataInicio    | 2026-09-20 14:00    |
      | dataFim       | 2026-09-20 18:00    |
      | unidadeId     | 102                 |
      | blocoUnidade  | Bloco A - Apto 102  |
      | placaVeiculo  | ABC-1234            |
      | observacoes   | Convidado de jantar |
    Então a resposta da IA deve confirmar a liberação do visitante "Carlos Eduardo" com sucesso
    E o registro deve ter o status "Agendado" no módulo de controle de acesso
    E a autorização deve estar vinculada ao Tenant "1" do condomínio

  Cenário: Morador pré-autoriza um prestador de serviço informando o nome da empresa
    Dado que o morador da unidade "204" no condomínio "1" está autenticado no sistema
    Quando o assistente de IA executa a ferramenta "AuthorizeGuest" para o prestador "Roberto Alencar", documento "987.654.321-11", tipo "PrestadorServico" e empresa "Manutenção Tech"
    Então a autorização de prestador de serviço deve ser criada com sucesso contendo a empresa "Manutenção Tech"

  Cenário: Falha de validação de domínio ao autorizar prestador de serviço sem informar a empresa
    Dado que o morador tenta cadastrar um "PrestadorServico" via assistente de IA
    Quando a ferramenta "AuthorizeGuest" é invocada sem o parâmetro obrigatório "empresa"
    Então o assistente de IA deve retornar uma mensagem de erro de validação de domínio
