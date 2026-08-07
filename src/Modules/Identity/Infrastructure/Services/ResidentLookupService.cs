using Microsoft.EntityFrameworkCore;
using Modules.Identity.Application.Services;
using Modules.Identity.Domain;
using Modules.Identity.Infrastructure.Persistence;

namespace Modules.Identity.Infrastructure.Services;

public class ResidentLookupService : IResidentLookupService
{
    private readonly IdentityDbContext _dbContext;

    public ResidentLookupService(IdentityDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ResidentLookupResultDto?> FindByPhoneE164Async(
        string phoneE164,
        int? tenantId = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(phoneE164))
        {
            return null;
        }

        var normalizedDigits = new string(phoneE164.Where(char.IsDigit).ToArray());
        var e164WithPlus = normalizedDigits.StartsWith('+') ? normalizedDigits : "+" + normalizedDigits;

        var query = _dbContext.Moradores.AsNoTracking();

        if (tenantId.HasValue && tenantId.Value > 0)
        {
            query = query.Where(m => m.TenantId == tenantId.Value);
        }
        else
        {
            // Busca cross-tenant no background consumer desabilitando os filtros globais de requisição HTTP
            query = query.IgnoreQueryFilters();
        }

        var morador = await query.FirstOrDefaultAsync(m =>
            m.TelefoneWhatsAppE164 == e164WithPlus ||
            m.TelefoneWhatsApp == phoneE164 ||
            m.TelefoneWhatsApp == normalizedDigits,
            cancellationToken);

        if (morador == null)
        {
            return null;
        }

        return new ResidentLookupResultDto(
            morador.TenantId,
            morador.CondoId,
            morador.Id,
            morador.UserId,
            morador.Nome,
            morador.TelefoneWhatsAppE164 ?? e164WithPlus);
    }
}
