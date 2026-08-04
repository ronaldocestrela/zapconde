# language: pt-BR
Funcionalidade: Menu de navegacao global do AppShell Stitch
  Como usuario autenticado do ZapCond
  Eu quero um menu lateral e topbar consistentes com a identidade Stitch
  Para navegar entre modulos conforme meu papel de acesso

  Contexto:
    Dado que o usuario esta autenticado com token valido
    E possui contexto de tenant ativo

  Cenario: Sindico ve menu completo de gestao e inteligencia
    Dado que o perfil ativo e "Sindico"
    Quando acessa qualquer pagina com AppShellLayout
    Entao deve ver sidebar com "Inicio", "Unidades e Moradores", "Financeiro", "Operacoes", "Portaria" e "WhatsApp / IA"
    E deve ver "Configuracoes" ancorado no rodape da sidebar

  Cenario: Morador ve menu reduzido sem gestao de unidades
    Dado que o perfil ativo e "Condomino"
    Quando acessa o dashboard
    Entao deve ver "Inicio", "Financeiro", "Operacoes" e "Portaria"
    E nao deve ver "Unidades e Moradores" nem "WhatsApp / IA"

  Cenario: Porteiro ve apenas modulos de acesso e inicio
    Dado que o perfil ativo e "Portaria"
    Quando acessa o dashboard
    Entao deve ver "Inicio" e "Portaria"
    E nao deve ver "Financeiro", "Operacoes" nem "Unidades e Moradores"

  Cenario: Item de menu ativo recebe destaque visual Stitch
    Dado que o perfil ativo e "Sindico"
    Quando navega para "/unidades"
    Entao o item "Unidades e Moradores" deve estar marcado como ativo com fundo primary

  Cenario: Usuario sem tenant nao ve sidebar de modulos
    Dado que o usuario nao possui contexto de tenant ativo
    Quando acessa o dashboard
    Entao nao deve ver sidebar de navegacao de modulos
    E deve ver topbar simplificada com badge "Sem tenant ativo"

  Cenario: Menu mobile abre drawer off-canvas
    Dado que a viewport e mobile
    E o perfil ativo e "Sindico"
    Quando toca no botao de menu hamburger
    Entao a sidebar deve abrir como drawer sobre overlay
    E ao selecionar um item o drawer deve fechar

  Cenario: Topbar exibe seletor de tenant, notificacoes e perfil
    Dado que o perfil ativo e "Administradora"
    Quando acessa o dashboard
    Entao deve ver TenantSwitcher no cabecalho
    E deve ver icone de notificacoes
    E deve ver menu de perfil com opcao de trocar perfil e sair

  Cenario: Rotas stub exibem placeholder Stitch em vez de 404
    Dado que o perfil ativo e "Sindico"
    Quando navega para "/financeiro"
    Entao deve ver pagina placeholder "Em breve" com identidade Stitch
