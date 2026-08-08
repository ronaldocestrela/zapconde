using System.Text.Json.Serialization;

namespace BuildingBlocks.Shared;

/// <summary>
/// Representa o resultado de uma operação sem dados de retorno.
/// Padrão obrigatório para todas as respostas de API conforme AGENTS.md.
/// </summary>
public class Result
{
    /// <summary>
    /// Indica se a operação foi bem-sucedida
    /// </summary>
    public bool IsSuccess { get; protected init; }

    /// <summary>
    /// Mensagem descritiva do resultado
    /// </summary>
    public string Message { get; protected init; }

    /// <summary>
    /// Lista de erros, quando aplicável
    /// </summary>
    public IEnumerable<string> Errors { get; protected init; }

    [JsonConstructor]
    public Result(bool isSuccess, string message, IEnumerable<string>? errors = null)
    {
        IsSuccess = isSuccess;
        Message = message;
        Errors = errors ?? Array.Empty<string>();
    }

    /// <summary>
    /// Cria um resultado de sucesso
    /// </summary>
    public static Result Success(string message = "Operação realizada com sucesso")
        => new(true, message);

    /// <summary>
    /// Cria um resultado de falha
    /// </summary>
    public static Result Failure(string message, IEnumerable<string>? errors = null)
        => new(false, message, errors);

    /// <summary>
    /// Cria um resultado de falha de validação
    /// </summary>
    public static Result ValidationFailure(IEnumerable<string> errors)
        => new(false, "Erro de validação", errors);

    /// <summary>
    /// Cria um resultado de falha de validação com mensagem customizada
    /// </summary>
    public static Result ValidationFailure(string message, IEnumerable<string> errors)
        => new(false, message, errors);
}
