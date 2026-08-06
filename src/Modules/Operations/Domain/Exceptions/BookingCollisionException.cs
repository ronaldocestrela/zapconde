namespace Modules.Operations.Domain.Exceptions;

/// <summary>
/// Exceção de domínio lançada quando ocorre colisão de horário em reservas de áreas comuns.
/// </summary>
public class BookingCollisionException : Exception
{
    public int AreaComumId { get; }
    public DateTime DataInicio { get; }
    public DateTime DataFim { get; }

    public BookingCollisionException(int areaComumId, DateTime dataInicio, DateTime dataFim)
        : base($"Já existe uma reserva no mesmo espaço (ID {areaComumId}) para o intervalo de {dataInicio:dd/MM/yyyy HH:mm} a {dataFim:dd/MM/yyyy HH:mm}.")
    {
        AreaComumId = areaComumId;
        DataInicio = dataInicio;
        DataFim = dataFim;
    }

    public BookingCollisionException(string message) : base(message)
    {
    }
}
