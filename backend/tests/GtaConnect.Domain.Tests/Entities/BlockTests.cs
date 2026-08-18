using GtaConnect.Domain.Entities;

namespace GtaConnect.Domain.Tests.Entities;

public class BlockTests
{
    [Fact]
    public void Create_ComParticipantesValidos_CriaBlockCorretamente()
    {
        var blockerProfileId = Guid.NewGuid();
        var blockedProfileId = Guid.NewGuid();

        var block = Block.Create(blockerProfileId, blockedProfileId);

        Assert.Equal(blockerProfileId, block.BlockerProfileId);
        Assert.Equal(blockedProfileId, block.BlockedProfileId);
    }

    [Fact]
    public void Create_ComMesmoPerfilNosDoisLados_LancaArgumentException()
    {
        var profileId = Guid.NewGuid();

        Assert.Throws<ArgumentException>(() => Block.Create(profileId, profileId));
    }

    [Fact]
    public void Create_ComPerfilVazio_LancaArgumentException()
    {
        Assert.Throws<ArgumentException>(() => Block.Create(Guid.Empty, Guid.NewGuid()));
    }
}
