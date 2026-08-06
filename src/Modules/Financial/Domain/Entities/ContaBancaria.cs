using BuildingBlocks.Shared.MultiTenancy;
using Modules.Financial.Domain.Enums;

namespace Modules.Financial.Domain.Entities;

/// <summary>
/// Conta Bancária Condominial para controle e conciliação bancária.
/// </summary>
public class ContaBancaria : ITenantScoped
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public int CondoId { get; set; }
    public string NomeBanco { get; set; } = string.Empty;
    public string CodigoBanco { get; set; } = string.Empty;
    public string Agencia { get; set; } = string.Empty;
    public string NumeroConta { get; set; } = string.Empty;
    public TipoContaBancaria TipoConta { get; set; } = TipoContaBancaria.Corrente;
    public decimal SaldoAtual { get; set; }
    public bool Ativa { get; set; } = true;
    public DateTime DataCriacao { get; set; } = DateTime.UtcNow;

    protected ContaBancaria() { }

    public static ContaBancaria Create(
        int tenantId,
        int condoId,
        string nomeBanco,
        string codigoBanco,
        string agencia,
        string numeroConta,
        TipoContaBancaria tipoConta = TipoContaBancaria.Corrente,
        decimal saldoInicial = 0)
    {
        if (tenantId <= 0) throw new ArgumentException("TenantId inválido.", nameof(tenantId));
        if (condoId <= 0) throw new ArgumentException("CondoId inválido.", nameof(condoId));
        if (string.IsNullOrWhiteSpace(nomeBanco)) throw new ArgumentException("Nome do Banco é obrigatório.", nameof(nomeBanco));
        if (string.IsNullOrWhiteSpace(numeroConta)) throw new ArgumentException("Número da conta é obrigatório.", nameof(numeroConta));

        return new ContaBancaria
        {
            TenantId = tenantId,
            CondoId = condoId,
            NomeBanco = nomeBanco,
            CodigoBanco = codigoBanco,
            Agencia = agencia,
            NumeroConta = numeroConta,
            TipoConta = tipoConta,
            SaldoAtual = saldoInicial,
            Ativa = true,
            DataCriacao = DateTime.UtcNow
        };
    }
}
