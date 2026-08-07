# language: pt-BR
Funcionalidade: Pipeline de RAG com Pgvector para Convenção e Regimento Interno
  Como síndico ou administradora do condomínio
  Quero cadastrar a Convenção, Regimento Interno e Regulamentos do condomínio
  Para que o assistente de IA possa responder dúvidas operacionais dos moradores via busca por similaridade vetorial (pgvector) com isolamento multi-tenant

  Cenário: Processamento e indexação vetorial de um novo Regimento Interno
    Dado que a administradora está autenticada no condomínio "1"
    E envia o documento do "Regimento Interno 2026" do tipo "RegimentoInterno" com o texto "É proibido barulho excessivo após as 22h nas áreas comuns."
    Quando o pipeline de RAG processa o documento
    Então o documento deve ser salvo no status ativo com a contagem de fragmentos maior que zero
    E os fragmentos de texto devem possuir embeddings vetoriais com 1536 dimensões persistidos no pgvector
    E o isolamento multi-tenant deve garantir que o condomínio "2" não tenha acesso aos fragmentos do condomínio "1"

  Cenário: Busca por similaridade vetorial de regras do condomínio
    Dado que o regimento interno do condomínio "1" possui regras sobre "Horário de uso da piscina até as 20h"
    Quando o morador pesquisa via busca vetorial a pergunta "Qual o horário limite para usar a piscina?"
    Então a consulta por similaridade vetorial no pgvector deve retornar o fragmento relevante da piscina
    E a pontuação de similaridade do primeiro resultado deve ser superior a 0.70
