using BuildingBlocks.Shared;
using Microsoft.EntityFrameworkCore;
using Modules.Identity.Application;
using Modules.Identity.Application.Dtos;
using Modules.Identity.Domain;
using Modules.Identity.Infrastructure.Persistence;

namespace Modules.Identity.Infrastructure.Services;

public sealed class CnpjLookupService(IdentityDbContext dbContext) : ICnpjLookupService
{
    public async Task<Result<CnpjStatusDto>> GetStatusAsync(string cnpj, CancellationToken ct = default)
    {
        var normalized = CnpjValidator.Normalize(cnpj);
        if (string.IsNullOrEmpty(normalized))
        {
            return Result<CnpjStatusDto>.ValidationFailure(["CNPJ é obrigatório."]);
        }

        if (!CnpjValidator.IsValid(normalized))
        {
            return Result<CnpjStatusDto>.ValidationFailure(["CNPJ inválido."]);
        }

        var exists = await dbContext.Administradoras
            .IgnoreQueryFilters()
            .AnyAsync(a => a.Cnpj == normalized, ct);

        var dto = new CnpjStatusDto
        {
            Cnpj = CnpjValidator.Format(normalized),
            IsAvailable = !exists,
            Status = exists ? "registered" : "available"
        };

        if (exists)
        {
            return Result<CnpjStatusDto>.Failure(
                "CNPJ já cadastrado no sistema. Verifique os dados ou solicite a recuperação de acesso.",
                [dto.Status]);
        }

        return Result<CnpjStatusDto>.Success(dto, "CNPJ disponível.");
    }
}
