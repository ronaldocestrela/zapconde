# language: pt-BR
Funcionalidade: Autenticacao Identity com JWT OpenIddict e claims multi-tenant
  Como usuario do SmartCondo
  Eu quero autenticar com email/senha, selecionar perfil e receber JWT com claims obrigatorias
  Para acessar recursos do condominio com isolamento por tenant

  Contexto:
    Dado que o modulo Identity esta configurado com OpenIddict e ASP.NET Core Identity
    E existe um usuario ativo com email "sindico@zapcond.com" e senha "Senha@123"
    E o usuario possui membership ativa no tenant "1" condominio "10" com role "Sindico"

  Cenario: Login com credenciais validas retorna tokens e perfis disponiveis
    Quando eu faco POST para "/api/auth/login" com email "sindico@zapcond.com" e senha "Senha@123"
    Entao o status HTTP deve ser 200 OK
    E a resposta deve conter "isSuccess" com valor "true"
    E a resposta deve conter accessToken e refreshToken
    E a resposta deve conter lista de perfis com tenantId, condoId e role

  Cenario: Login com credenciais invalidas retorna erro Stitch
    Quando eu faco POST para "/api/auth/login" com email "sindico@zapcond.com" e senha "errada"
    Entao o status HTTP deve ser 401 Unauthorized
    E a mensagem deve indicar credenciais invalidas

  Cenario: Login de usuario bloqueado retorna erro Stitch
    Dado que o usuario "bloqueado@zapcond.com" esta inativo
    Quando eu faco POST para "/api/auth/login" com email "bloqueado@zapcond.com" e senha "Senha@123"
    Entao o status HTTP deve ser 403 Forbidden
    E a mensagem deve indicar usuario bloqueado

  Cenario: Selecao de perfil emite JWT com claims TenantId CondoId UserId Role
    Dado que possuo tokens validos de login
    Quando eu faco POST para "/api/auth/select-profile" com membershipId valido
    Entao o status HTTP deve ser 200 OK
    E o accessToken decodificado deve conter claim "TenantId"
    E o accessToken decodificado deve conter claim "CondoId"
    E o accessToken decodificado deve conter claim "UserId"
    E o accessToken decodificado deve conter claim "Role" com valor "Sindico"

  Cenario: Selecao de perfil invalido retorna erro
    Dado que possuo tokens validos de login
    Quando eu faco POST para "/api/auth/select-profile" com membershipId inexistente
    Entao o status HTTP deve ser 404 Not Found

  Cenario: Recuperacao de senha retorna mensagem generica de sucesso
    Quando eu faco POST para "/api/auth/forgot-password" com email "sindico@zapcond.com"
    Entao o status HTTP deve ser 200 OK
    E a mensagem deve indicar verificacao de e-mail

  Cenario: Recuperacao de senha com e-mail invalido retorna validacao
    Quando eu faco POST para "/api/auth/forgot-password" com email "invalido"
    Entao o status HTTP deve ser 400 Bad Request

  Cenario: Refresh token valido renova access token
    Dado que possuo refresh token valido
    Quando eu faco POST para "/api/auth/refresh" com o refresh token
    Entao o status HTTP deve be 200 OK
    E a resposta deve conter novo accessToken

  Cenario: Perfil Porteiro mapeia para role Portaria no JWT
    Dado que o usuario possui membership com role "Portaria"
    Quando eu seleciono esse perfil
    Entao o accessToken decodificado deve conter claim "Role" com valor "Portaria"

  Cenario: Perfil Morador mapeia para role Condomino no JWT
    Dado que o usuario possui membership com role "Condomino"
    Quando eu seleciono esse perfil
    Entao o accessToken decodificado deve conter claim "Role" com valor "Condomino"
