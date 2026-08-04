using Modules.Identity.Domain;

namespace Modules.Identity.Application.Dtos;

public sealed class BlockDto
{
    public int Id { get; set; }

    public string Codigo { get; set; } = string.Empty;

    public string Nome { get; set; } = string.Empty;

    public int Ordem { get; set; }
}

public sealed class CreateBlockRequestDto
{
    public string Codigo { get; set; } = string.Empty;

    public string? Nome { get; set; }

    public int Ordem { get; set; }
}

public sealed class CreateUnitRequestDto
{
    public int? BlocoId { get; set; }

    public string? BlocoCodigo { get; set; }

    public string Numero { get; set; } = string.Empty;

    public UnidadeStatus Status { get; set; } = UnidadeStatus.Vaga;

    public string MoradorNome { get; set; } = string.Empty;

    public string MoradorCpf { get; set; } = string.Empty;

    public string MoradorEmail { get; set; } = string.Empty;

    public string MoradorTelefone { get; set; } = string.Empty;

    public PapelVinculo Papel { get; set; } = PapelVinculo.Proprietario;

    public DateTime DataInicio { get; set; } = DateTime.UtcNow;

    public List<string> Dependencias { get; set; } = [];
}

public sealed class UpdateUnitRequestDto
{
    public UnidadeStatus Status { get; set; }

    public string MoradorNome { get; set; } = string.Empty;

    public string MoradorCpf { get; set; } = string.Empty;

    public string MoradorEmail { get; set; } = string.Empty;

    public string MoradorTelefone { get; set; } = string.Empty;

    public PapelVinculo Papel { get; set; }

    public List<string> Dependencias { get; set; } = [];
}

public sealed class UnitListItemDto
{
    public int UnitId { get; set; }

    public int BlocoId { get; set; }

    public string BlocoCodigo { get; set; } = string.Empty;

    public string Numero { get; set; } = string.Empty;

    public UnidadeStatus Status { get; set; }

    public string? MoradorNome { get; set; }

    public PapelVinculo? Papel { get; set; }

    public string? MoradorTelefone { get; set; }

    public DateTime? DataInicio { get; set; }

    public int? MoradorId { get; set; }

    public int? VinculoId { get; set; }
}

public sealed class UnitCreatedDto
{
    public int UnitId { get; set; }

    public int ResidentId { get; set; }

    public int VinculoId { get; set; }
}

public sealed class TransferOwnershipRequestDto
{
    public DateTime DataEncerramento { get; set; }

    public string Motivo { get; set; } = string.Empty;

    public PapelVinculo Papel { get; set; } = PapelVinculo.Proprietario;

    public string NovoMoradorNome { get; set; } = string.Empty;

    public string NovoMoradorCpf { get; set; } = string.Empty;

    public string NovoMoradorEmail { get; set; } = string.Empty;

    public string NovoMoradorTelefone { get; set; } = string.Empty;

    public DateTime DataInicio { get; set; } = DateTime.UtcNow;

    public List<string> Dependencias { get; set; } = [];
}

public sealed class UnitHistoryItemDto
{
    public int VinculoId { get; set; }

    public string MoradorNome { get; set; } = string.Empty;

    public PapelVinculo Papel { get; set; }

    public DateTime DataInicio { get; set; }

    public DateTime? DataFim { get; set; }

    public string? MotivoEncerramento { get; set; }

    public bool IsActive { get; set; }

    public string? CreatedByUserId { get; set; }

    public List<string> Dependencias { get; set; } = [];
}

public sealed class UnitListQueryDto
{
    public string? Q { get; set; }

    public int? BlockId { get; set; }

    public UnidadeStatus? Status { get; set; }

    public PapelVinculo? Papel { get; set; }
}

public sealed class ImportPreviewRowDto
{
    public int RowNumber { get; set; }

    public string BlocoCodigo { get; set; } = string.Empty;

    public string Numero { get; set; } = string.Empty;

    public string MoradorNome { get; set; } = string.Empty;

    public string MoradorCpf { get; set; } = string.Empty;

    public string MoradorEmail { get; set; } = string.Empty;

    public string MoradorTelefone { get; set; } = string.Empty;

    public PapelVinculo Papel { get; set; }

    public bool IsValid { get; set; }

    public List<string> Errors { get; set; } = [];
}

public sealed class ImportPreviewResultDto
{
    public int TotalRows { get; set; }

    public int ValidRows { get; set; }

    public int InvalidRows { get; set; }

    public List<ImportPreviewRowDto> Rows { get; set; } = [];
}

public sealed class ImportCommitRequestDto
{
    public List<ImportPreviewRowDto> Rows { get; set; } = [];
}

public sealed class ImportCommitResultDto
{
    public int ImportedCount { get; set; }

    public int SkippedCount { get; set; }
}
