using FluentAssertions;
using Modules.Operations.Domain.Entities;
using Modules.Operations.Domain.Enums;
using Modules.Operations.Domain.Exceptions;
using Xunit;

namespace Tests.Unit.Operations;

public class OcorrenciaDomainTests
{
    private const int TenantId = 1;
    private const int CondoId = 100;
    private const string MoradorId = "user-morador-1";
    private const string MoradorNome = "João Silva";

    [Fact]
    public void Create_Should_Initialize_Ocorrencia_With_Status_Aberta_And_HistoricoInicial()
    {
        // Act
        var ocorrencia = Ocorrencia.Create(
            tenantId: TenantId,
            condoId: CondoId,
            moradorId: MoradorId,
            moradorNome: MoradorNome,
            titulo: "Barulho excessivo",
            descricao: "Música alta no Bloco B apto 302 após as 22h",
            categoria: CategoriaOcorrencia.Barulho,
            prioridade: PrioridadeOcorrencia.Media,
            localizacao: "Bloco B - Apto 302"
        );

        // Assert
        ocorrencia.Should().NotBeNull();
        ocorrencia.Id.Should().NotBeEmpty();
        ocorrencia.TenantId.Should().Be(TenantId);
        ocorrencia.CondoId.Should().Be(CondoId);
        ocorrencia.Status.Should().Be(StatusOcorrencia.Aberta);
        ocorrencia.DataConclusao.Should().BeNull();
        ocorrencia.Historico.Should().HaveCount(1);

        var historico = ocorrencia.Historico.First();
        historico.StatusAnterior.Should().BeNull();
        historico.StatusNovo.Should().Be(StatusOcorrencia.Aberta);
        historico.Comentario.Should().Be("Ocorrencia aberta pelo morador");
    }

    [Fact]
    public void AdicionarAnexo_Should_Add_Photo_To_Ocorrencia()
    {
        // Arrange
        var ocorrencia = Ocorrencia.Create(TenantId, CondoId, MoradorId, MoradorNome, "Infiltração", "Vazamento no teto", CategoriaOcorrencia.Manutencao, PrioridadeOcorrencia.Alta, "Garagem Subsolo");

        // Act
        var anexo = ocorrencia.AdicionarAnexo(
            url: "/uploads/tickets/foto1.jpg",
            nomeArquivo: "foto1.jpg",
            contentType: "image/jpeg",
            tamanhoBytes: 1024500,
            uploadPorUserId: MoradorId
        );

        // Assert
        ocorrencia.Anexos.Should().HaveCount(1);
        anexo.Url.Should().Be("/uploads/tickets/foto1.jpg");
        anexo.TenantId.Should().Be(TenantId);
        anexo.OcorrenciaId.Should().Be(ocorrencia.Id);
    }

    [Fact]
    public void AtualizarStatus_Should_Transition_Validly_And_Record_Historico()
    {
        // Arrange
        var ocorrencia = Ocorrencia.Create(TenantId, CondoId, MoradorId, MoradorNome, "Portão Quebrado", "Portão social não fecha", CategoriaOcorrencia.Seguranca, PrioridadeOcorrencia.Urgente, "Portaria Principal");

        // Act 1: Aberta -> EmAndamento
        ocorrencia.AtualizarStatus(StatusOcorrencia.EmAndamento, "Zelador assumiu a manutenção", "user-zelador", "Zelador Carlos");

        // Assert 1
        ocorrencia.Status.Should().Be(StatusOcorrencia.EmAndamento);
        ocorrencia.Historico.Should().HaveCount(2);

        // Act 2: EmAndamento -> Resolvida
        ocorrencia.AtualizarStatus(StatusOcorrencia.Resolvida, "Troca de mola concluída", "user-zelador", "Zelador Carlos", "Troca de mola hidráulica concluída com sucesso.");

        // Assert 2
        ocorrencia.Status.Should().Be(StatusOcorrencia.Resolvida);
        ocorrencia.DataConclusao.Should().NotBeNull();
        ocorrencia.ObservacaoResolucao.Should().Be("Troca de mola hidráulica concluída com sucesso.");
        ocorrencia.Historico.Should().HaveCount(3);
    }

    [Fact]
    public void AtualizarStatus_Should_ThrowException_When_Transition_Is_Invalid()
    {
        // Arrange
        var ocorrencia = Ocorrencia.Create(TenantId, CondoId, MoradorId, MoradorNome, "Lixo no corredor", "Saco de lixo deixado no 4o andar", CategoriaOcorrencia.Limpeza, PrioridadeOcorrencia.Baixa, "4o Andar Bloco A");
        ocorrencia.AtualizarStatus(StatusOcorrencia.EmAndamento, "Iniciado", "user-zelador", "Zelador Carlos");
        ocorrencia.AtualizarStatus(StatusOcorrencia.Resolvida, "Limpo", "user-zelador", "Zelador Carlos");

        // Act & Assert (Resolvida -> EmAndamento não é permitido)
        var action = () => ocorrencia.AtualizarStatus(StatusOcorrencia.EmAndamento, "Tentando reabrir", "user-zelador", "Zelador Carlos");
        action.Should().Throw<InvalidOcorrenciaStatusTransitionException>();
    }
}
