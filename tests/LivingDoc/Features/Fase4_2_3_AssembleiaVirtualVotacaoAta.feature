# language: pt-BR
Funcionalidade: Assembleias Virtuais, Pautas de Votação e Ata Oficial
  Como Síndico, Administradora ou Morador do condomínio
  Quero participar de assembleias virtuais, votar em pautas com garantia de unicidade por unidade e visualizar a ata final
  Para garantir a transparência democrática, apuração auditável de votos e registro oficial dos acordos condominiais

  Cenario: Cadastro de assembleia virtual com pautas de votação
    Dado que o síndico cadastra uma assembleia virtual "Assembleia Geral Ordinária 2026" do tipo "Ordinaria"
    E adiciona a pauta "Aprovação das Contas do Exercício Anterior" com tipo de votação "MaioriaSimples"
    E adiciona a pauta "Eleição do Novo Síndico" com tipo de votação "MaioriaSimples"
    Quando a assembleia é salva no sistema
    Então a assembleia deve possuir status "Agendada"
    E deve conter 2 pautas cadastradas
    E deve possuir isolamento multi-tenant ativo

  Cenario: Iniciar assembleia e registrar voto válido de morador
    Dado que a assembleia "Assembleia Geral Ordinária 2026" está com status "EmAndamento"
    E a pauta "Aprovação das Contas do Exercício Anterior" está "Aberta"
    Quando o morador da unidade "101" registra o voto "Sim" na pauta
    Então o voto deve ser registrado com sucesso
    E o total de votos computados para a opção "Sim" na pauta deve ser 1

  Cenario: Impedir voto duplicado da mesma unidade habitacional na mesma pauta
    Dado que a unidade "101" já votou "Sim" na pauta "Aprovação das Contas do Exercício Anterior"
    Quando o morador tentar registrar um novo voto "Não" para a mesma unidade "101" na pauta
    Então o sistema deve rejeitar o voto e lançar exceção de voto duplicado
    E a contagem de votos da pauta deve permanecer inalterada

  Cenario: Impedir votação quando a assembleia estiver encerrada
    Dado que a assembleia "Assembleia Extraordinária de Reforma" está "Encerrada"
    Quando o morador tentar votar na pauta "Aprovação de Quota Extra"
    Então o sistema deve rejeitar a operação indicando que a assembleia está encerrada

  Cenario: Encerrar assembleia, apurar resultado das pautas e gerar Ata Oficial
    Dado que a assembleia "Assembleia Geral Ordinária 2026" está com status "EmAndamento"
    E 3 unidades votaram na pauta "Aprovação das Contas do Exercício Anterior" (2 "Sim", 1 "Não")
    Quando o síndico encerra a assembleia
    Então o status da assembleia deve mudar para "Encerrada"
    E o texto da Ata Oficial deve ser gerado contendo o quórum total e o resultado apurado das pautas
