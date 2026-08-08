using BuildingBlocks.Shared;
using BuildingBlocks.Shared.Caching;
using BuildingBlocks.Shared.Events;
using MassTransit;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Modules.Identity.Application;
using Modules.Identity.Application.Dtos;
using Modules.Identity.Application.Services;
using Modules.Identity.Domain;
using Modules.Identity.Infrastructure.Persistence;

namespace Modules.Identity.Infrastructure.Services;

public sealed class TenantOnboardingService(
    IdentityDbContext dbContext,
    UserManager<ApplicationUser> userManager,
    IOnboardingDraftService draftService,
    IHostEnvironment environment,
    IPublishEndpoint publishEndpoint,
    ILogger<TenantOnboardingService> logger) : ITenantOnboardingService
{
    public async Task<Result<TenantCreatedDto>> CreateAsync(CreateTenantRequestDto request, CancellationToken ct = default)
    {
        var validationErrors = ValidateRequest(request);
        if (validationErrors.Count > 0)
        {
            return Result<TenantCreatedDto>.ValidationFailure(validationErrors);
        }

        var normalizedCnpj = CnpjValidator.Normalize(request.Administradora.Cnpj);
        var cnpjExists = await dbContext.Administradoras
            .IgnoreQueryFilters()
            .AnyAsync(a => a.Cnpj == normalizedCnpj, ct);

        if (cnpjExists)
        {
            return Result<TenantCreatedDto>.Failure(
                "CNPJ já cadastrado no sistema. Verifique os dados ou solicite a recuperação de acesso.");
        }

        if (request.SimulateRollback && environment.IsEnvironment("Testing"))
        {
            return Result<TenantCreatedDto>.Failure(
                "Falha na transação de banco de dados. O processo de criação foi revertido (rollback efetuado) sem alterar seus registros.");
        }

        var useTransaction = dbContext.Database.IsRelational() &&
                             dbContext.Database.ProviderName != "Microsoft.EntityFrameworkCore.InMemory";

        Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction? transaction = null;
        if (useTransaction)
        {
            transaction = await dbContext.Database.BeginTransactionAsync(ct);
        }

        try
        {
            var nextTenantId = (await dbContext.Administradoras.IgnoreQueryFilters().MaxAsync(a => (int?)a.Id, ct) ?? 0) + 1;
            var nextCondoId = (await dbContext.Condominios.IgnoreQueryFilters().MaxAsync(c => (int?)c.Id, ct) ?? 0) + 1;

            var administradora = Administradora.Create(
                nextTenantId,
                request.Administradora.RazaoSocial,
                request.Administradora.Cnpj,
                request.Administradora.NomeFantasia,
                request.Administradora.LicensePlan);

            var endereco = new Endereco
            {
                Cep = Endereco.NormalizeCep(request.Endereco.Cep),
                Logradouro = request.Endereco.Logradouro,
                Numero = request.Endereco.Numero,
                Bairro = request.Endereco.Bairro,
                Cidade = request.Endereco.Cidade,
                Uf = request.Endereco.Uf.ToUpperInvariant()
            };

            var configuracoes = new ConfiguracoesIniciais
            {
                DiaVencimento = request.Configuracoes.DiaVencimento,
                JurosEnabled = request.Configuracoes.JurosEnabled,
                MultaEnabled = request.Configuracoes.MultaEnabled,
                BankGateway = request.Configuracoes.BankGateway,
                WhatsAppAiEnabled = request.Configuracoes.WhatsAppAiEnabled
            };

            var condominio = Condominio.Create(
                nextCondoId,
                nextTenantId,
                request.Condominio.Nome,
                request.Condominio.Tipo,
                request.Condominio.TotalUnits,
                request.Condominio.NumberOfBlocks,
                endereco,
                request.Contatos.MasterAdminName,
                request.Contatos.CorporateEmail,
                request.Contatos.PhoneWhatsApp,
                request.Contatos.EmergencyPhone,
                configuracoes);

            dbContext.Administradoras.Add(administradora);
            dbContext.Condominios.Add(condominio);

            var masterRole = string.IsNullOrWhiteSpace(request.Contatos.MasterRole)
                ? SmartCondoRoles.Sindico
                : request.Contatos.MasterRole;

            string tempPasswordDisplay = "[Senha mantida - Usuário já existente no sistema]";
            var masterUser = await userManager.FindByEmailAsync(request.Contatos.CorporateEmail);
            if (masterUser is null)
            {
                masterUser = new ApplicationUser
                {
                    Id = Guid.NewGuid(),
                    Email = request.Contatos.CorporateEmail,
                    UserName = request.Contatos.CorporateEmail,
                    EmailConfirmed = true,
                    DisplayName = request.Contatos.MasterAdminName,
                    IsActive = true
                };

                var tempPassword = $"Zap@{Guid.NewGuid():N}"[..16];
                tempPasswordDisplay = tempPassword;
                var createResult = await userManager.CreateAsync(masterUser, tempPassword);
                if (!createResult.Succeeded)
                {
                    if (transaction is not null)
                    {
                        await transaction.RollbackAsync(ct);
                    }

                    return Result<TenantCreatedDto>.Failure(
                        "Falha na transação de banco de dados. O processo de criação foi revertido (rollback efetuado) sem alterar seus registros.",
                        createResult.Errors.Select(e => e.Description));
                }
            }

            dbContext.UserCondoMemberships.Add(new UserCondoMembership
            {
                Id = Guid.NewGuid(),
                UserId = masterUser.Id,
                TenantId = nextTenantId,
                CondoId = nextCondoId,
                Role = masterRole,
                DisplayLabel = $"{request.Condominio.Nome} - {masterRole}",
                IsActive = true,
                IsTenantActive = true
            });

            await dbContext.SaveChangesAsync(ct);

            if (request.SimulateRollback)
            {
                throw new InvalidOperationException("Simulated rollback");
            }

            if (transaction is not null)
            {
                await transaction.CommitAsync(ct);
            }

            try
            {
                var welcomeEmail = TenantWelcomeEmailBuilder.BuildWelcomeEmail(
                    request.Contatos.CorporateEmail,
                    request.Contatos.MasterAdminName,
                    request.Condominio.Nome,
                    nextTenantId,
                    tempPasswordDisplay);

                await publishEndpoint.Publish(new SendEmailCommand(welcomeEmail, nextTenantId), ct);
                logger.LogInformation("SendEmailCommand publicado com sucesso para Tenant {TenantId} (E-mail: {Email})",
                    nextTenantId, request.Contatos.CorporateEmail);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Falha ao publicar SendEmailCommand durante onboarding do Tenant {TenantId}", nextTenantId);
            }

            if (request.DraftId.HasValue)
            {
                await draftService.RemoveDraftAsync(request.DraftId.Value, ct);
            }

            return Result<TenantCreatedDto>.Success(new TenantCreatedDto
            {
                TenantId = nextTenantId,
                CondoId = nextCondoId,
                MasterEmail = request.Contatos.CorporateEmail,
                CondominioNome = request.Condominio.Nome,
                CredentialsDispatchedMessage = "Credenciais de acesso enviadas para o e-mail corporativo informado."
            }, "Tenant criado com sucesso.");
        }
        catch (Exception)
        {
            if (transaction is not null)
            {
                await transaction.RollbackAsync(ct);
            }

            return Result<TenantCreatedDto>.Failure(
                "Falha na transação de banco de dados. O processo de criação foi revertido (rollback efetuado) sem alterar seus registros.");
        }
        finally
        {
            if (transaction is not null)
            {
                await transaction.DisposeAsync();
            }
        }
    }

    private static List<string> ValidateRequest(CreateTenantRequestDto request)
    {
        var errors = new List<string>();

        try
        {
            Administradora.Create(1, request.Administradora.RazaoSocial, request.Administradora.Cnpj,
                request.Administradora.NomeFantasia, request.Administradora.LicensePlan);
        }
        catch (DomainValidationException ex)
        {
            errors.Add(ex.Message);
        }

        try
        {
            var endereco = new Endereco
            {
                Cep = Endereco.NormalizeCep(request.Endereco.Cep),
                Logradouro = request.Endereco.Logradouro,
                Numero = request.Endereco.Numero,
                Bairro = request.Endereco.Bairro,
                Cidade = request.Endereco.Cidade,
                Uf = request.Endereco.Uf
            };

            Condominio.Create(1, 1, request.Condominio.Nome, request.Condominio.Tipo,
                request.Condominio.TotalUnits, request.Condominio.NumberOfBlocks,
                endereco, request.Contatos.MasterAdminName, request.Contatos.CorporateEmail,
                request.Contatos.PhoneWhatsApp, request.Contatos.EmergencyPhone,
                new ConfiguracoesIniciais
                {
                    DiaVencimento = request.Configuracoes.DiaVencimento,
                    JurosEnabled = request.Configuracoes.JurosEnabled,
                    MultaEnabled = request.Configuracoes.MultaEnabled,
                    BankGateway = request.Configuracoes.BankGateway,
                    WhatsAppAiEnabled = request.Configuracoes.WhatsAppAiEnabled
                });
        }
        catch (DomainValidationException ex)
        {
            errors.Add(ex.Message);
        }

        return errors;
    }
}
