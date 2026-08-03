namespace BuildingBlocks.Shared;

/// <summary>
/// Representa o resultado de uma operação com dados de retorno tipados.
/// Padrão obrigatório para todas as respostas de API conforme AGENTS.md.
/// </summary>
/// <typeparam name="T">Tipo do dado retornado em caso de sucesso</typeparam>
public class Result<T> : Result
{
    /// <summary>
    /// Dados retornados pela operação
    /// </summary>
    public T? Data { get; private init; }

    private Result(bool isSuccess, string message, T? data = default, IEnumerable<string>? errors = null)
        : base(isSuccess, message, errors)
    {
        Data = data;
    }

    /// <summary>
    /// Cria um resultado de sucesso com dados
    /// </summary>
    public static Result<T> Success(T data, string message = "Operação realizada com sucesso")
        => new(true, message, data);

    /// <summary>
    /// Cria um resultado de falha sem dados
    /// </summary>
    public new static Result<T> Failure(string message, IEnumerable<string>? errors = null)
        => new(false, message, default, errors);

    /// <summary>
    /// Cria um resultado de falha de validação sem dados
    /// </summary>
    public new static Result<T> ValidationFailure(IEnumerable<string> errors)
        => new(false, "Erro de validação", default, errors);

    /// <summary>
    /// Cria um resultado de falha de validação com mensagem customizada
    /// </summary>
    public new static Result<T> ValidationFailure(string message, IEnumerable<string> errors)
        => new(false, message, default, errors);
}
