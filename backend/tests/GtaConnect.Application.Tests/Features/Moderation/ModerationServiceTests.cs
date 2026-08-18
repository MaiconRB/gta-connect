using GtaConnect.Application.Common.Interfaces;
using GtaConnect.Application.Features.Moderation;
using GtaConnect.Domain.Common.Exceptions;
using GtaConnect.Domain.Entities;
using GtaConnect.Domain.Enums;
using Moq;

namespace GtaConnect.Application.Tests.Features.Moderation;

public class ModerationServiceTests
{
    private readonly Mock<IBlockRepository> _blockRepositoryMock = new();
    private readonly Mock<IReportRepository> _reportRepositoryMock = new();
    private readonly Mock<IPlayerProfileRepository> _playerProfileRepositoryMock = new();
    private readonly ModerationService _sut;

    public ModerationServiceTests()
    {
        _sut = new ModerationService(_blockRepositoryMock.Object, _reportRepositoryMock.Object, _playerProfileRepositoryMock.Object);
    }

    private static PlayerProfile CreateValidProfile(Guid userId, string displayName = "Jogador") =>
        PlayerProfile.Create(userId, displayName, Platform.Ps5, GameTitle.GtaV);

    [Fact]
    public async Task BlockAsync_SemBloqueioExistente_CriaNovoBlock()
    {
        var userId = Guid.NewGuid();
        var myProfile = CreateValidProfile(userId);
        var targetProfile = CreateValidProfile(Guid.NewGuid(), "Alvo");

        _playerProfileRepositoryMock.Setup(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(myProfile);
        _playerProfileRepositoryMock.Setup(r => r.GetByIdAsync(targetProfile.Id, It.IsAny<CancellationToken>())).ReturnsAsync(targetProfile);
        _blockRepositoryMock.Setup(r => r.FindAsync(myProfile.Id, targetProfile.Id, It.IsAny<CancellationToken>())).ReturnsAsync((Block?)null);

        await _sut.BlockAsync(userId, targetProfile.Id);

        _blockRepositoryMock.Verify(r => r.AddAsync(It.Is<Block>(b => b.BlockerProfileId == myProfile.Id && b.BlockedProfileId == targetProfile.Id), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task BlockAsync_ComBloqueioJaExistente_NaoCriaDuplicadoNemLancaErro()
    {
        var userId = Guid.NewGuid();
        var myProfile = CreateValidProfile(userId);
        var targetProfile = CreateValidProfile(Guid.NewGuid(), "Alvo");
        var existingBlock = Block.Create(myProfile.Id, targetProfile.Id);

        _playerProfileRepositoryMock.Setup(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(myProfile);
        _playerProfileRepositoryMock.Setup(r => r.GetByIdAsync(targetProfile.Id, It.IsAny<CancellationToken>())).ReturnsAsync(targetProfile);
        _blockRepositoryMock.Setup(r => r.FindAsync(myProfile.Id, targetProfile.Id, It.IsAny<CancellationToken>())).ReturnsAsync(existingBlock);

        await _sut.BlockAsync(userId, targetProfile.Id);

        _blockRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Block>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task BlockAsync_ComAlvoInexistente_LancaNotFoundException()
    {
        var userId = Guid.NewGuid();
        var myProfile = CreateValidProfile(userId);
        var targetProfileId = Guid.NewGuid();

        _playerProfileRepositoryMock.Setup(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(myProfile);
        _playerProfileRepositoryMock.Setup(r => r.GetByIdAsync(targetProfileId, It.IsAny<CancellationToken>())).ReturnsAsync((PlayerProfile?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _sut.BlockAsync(userId, targetProfileId));
    }

    [Fact]
    public async Task UnblockAsync_ComBloqueioExistente_RemoveOBlock()
    {
        var userId = Guid.NewGuid();
        var myProfile = CreateValidProfile(userId);
        var targetProfileId = Guid.NewGuid();
        var existingBlock = Block.Create(myProfile.Id, targetProfileId);

        _playerProfileRepositoryMock.Setup(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(myProfile);
        _blockRepositoryMock.Setup(r => r.FindAsync(myProfile.Id, targetProfileId, It.IsAny<CancellationToken>())).ReturnsAsync(existingBlock);

        await _sut.UnblockAsync(userId, targetProfileId);

        _blockRepositoryMock.Verify(r => r.RemoveAsync(existingBlock, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UnblockAsync_SemBloqueioExistente_NaoLancaErro()
    {
        var userId = Guid.NewGuid();
        var myProfile = CreateValidProfile(userId);
        var targetProfileId = Guid.NewGuid();

        _playerProfileRepositoryMock.Setup(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(myProfile);
        _blockRepositoryMock.Setup(r => r.FindAsync(myProfile.Id, targetProfileId, It.IsAny<CancellationToken>())).ReturnsAsync((Block?)null);

        await _sut.UnblockAsync(userId, targetProfileId);

        _blockRepositoryMock.Verify(r => r.RemoveAsync(It.IsAny<Block>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ReportAsync_ComDadosValidos_PersisteReport()
    {
        var userId = Guid.NewGuid();
        var myProfile = CreateValidProfile(userId);
        var targetProfile = CreateValidProfile(Guid.NewGuid(), "Alvo");

        _playerProfileRepositoryMock.Setup(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(myProfile);
        _playerProfileRepositoryMock.Setup(r => r.GetByIdAsync(targetProfile.Id, It.IsAny<CancellationToken>())).ReturnsAsync(targetProfile);

        await _sut.ReportAsync(userId, targetProfile.Id, ReportReason.Assedio, "Mandou mensagem ofensiva");

        _reportRepositoryMock.Verify(
            r => r.AddAsync(
                It.Is<Report>(rep => rep.ReporterProfileId == myProfile.Id && rep.ReportedProfileId == targetProfile.Id && rep.Reason == ReportReason.Assedio),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ReportAsync_ComAlvoInexistente_LancaNotFoundException()
    {
        var userId = Guid.NewGuid();
        var myProfile = CreateValidProfile(userId);
        var targetProfileId = Guid.NewGuid();

        _playerProfileRepositoryMock.Setup(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(myProfile);
        _playerProfileRepositoryMock.Setup(r => r.GetByIdAsync(targetProfileId, It.IsAny<CancellationToken>())).ReturnsAsync((PlayerProfile?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _sut.ReportAsync(userId, targetProfileId, ReportReason.Spam, null));
    }
}
