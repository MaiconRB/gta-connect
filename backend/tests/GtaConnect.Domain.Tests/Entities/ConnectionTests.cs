using GtaConnect.Domain.Entities;
using GtaConnect.Domain.Enums;

namespace GtaConnect.Domain.Tests.Entities;

public class ConnectionTests
{
    [Fact]
    public void Create_ComParticipantesValidos_CriaComStatusPending()
    {
        var requesterProfileId = Guid.NewGuid();
        var addresseeProfileId = Guid.NewGuid();

        var connection = Connection.Create(requesterProfileId, addresseeProfileId);

        Assert.Equal(requesterProfileId, connection.RequesterProfileId);
        Assert.Equal(addresseeProfileId, connection.AddresseeProfileId);
        Assert.Equal(ConnectionStatus.Pending, connection.Status);
        Assert.Null(connection.RespondedAtUtc);
    }

    [Fact]
    public void Create_ComMesmoPerfilNosDoisLados_LancaArgumentException()
    {
        var profileId = Guid.NewGuid();

        Assert.Throws<ArgumentException>(() => Connection.Create(profileId, profileId));
    }

    [Fact]
    public void Accept_ChamadoPeloAddressee_MudaParaAccepted()
    {
        var requesterProfileId = Guid.NewGuid();
        var addresseeProfileId = Guid.NewGuid();
        var connection = Connection.Create(requesterProfileId, addresseeProfileId);

        connection.Accept(addresseeProfileId);

        Assert.Equal(ConnectionStatus.Accepted, connection.Status);
        Assert.NotNull(connection.RespondedAtUtc);
    }

    [Fact]
    public void Accept_ChamadoPeloRequester_LancaArgumentException()
    {
        var requesterProfileId = Guid.NewGuid();
        var addresseeProfileId = Guid.NewGuid();
        var connection = Connection.Create(requesterProfileId, addresseeProfileId);

        Assert.Throws<ArgumentException>(() => connection.Accept(requesterProfileId));
    }

    [Fact]
    public void Decline_ChamadoPeloAddressee_MudaParaDeclined()
    {
        var requesterProfileId = Guid.NewGuid();
        var addresseeProfileId = Guid.NewGuid();
        var connection = Connection.Create(requesterProfileId, addresseeProfileId);

        connection.Decline(addresseeProfileId);

        Assert.Equal(ConnectionStatus.Declined, connection.Status);
    }

    [Fact]
    public void Decline_ChamadoPeloRequester_TambemFunciona()
    {
        var requesterProfileId = Guid.NewGuid();
        var addresseeProfileId = Guid.NewGuid();
        var connection = Connection.Create(requesterProfileId, addresseeProfileId);

        connection.Decline(requesterProfileId);

        Assert.Equal(ConnectionStatus.Declined, connection.Status);
    }

    [Fact]
    public void Decline_ChamadoPorQuemNaoParticipa_LancaArgumentException()
    {
        var connection = Connection.Create(Guid.NewGuid(), Guid.NewGuid());

        Assert.Throws<ArgumentException>(() => connection.Decline(Guid.NewGuid()));
    }

    [Fact]
    public void Accept_ComConexaoJaAceita_LancaArgumentException()
    {
        var requesterProfileId = Guid.NewGuid();
        var addresseeProfileId = Guid.NewGuid();
        var connection = Connection.Create(requesterProfileId, addresseeProfileId);
        connection.Accept(addresseeProfileId);

        Assert.Throws<ArgumentException>(() => connection.Accept(addresseeProfileId));
    }

    [Fact]
    public void Decline_ComConexaoJaDeclinada_LancaArgumentException()
    {
        var requesterProfileId = Guid.NewGuid();
        var addresseeProfileId = Guid.NewGuid();
        var connection = Connection.Create(requesterProfileId, addresseeProfileId);
        connection.Decline(addresseeProfileId);

        Assert.Throws<ArgumentException>(() => connection.Decline(requesterProfileId));
    }

    [Fact]
    public void GetOtherParticipantId_RetornaOOutroParticipante()
    {
        var requesterProfileId = Guid.NewGuid();
        var addresseeProfileId = Guid.NewGuid();
        var connection = Connection.Create(requesterProfileId, addresseeProfileId);

        Assert.Equal(addresseeProfileId, connection.GetOtherParticipantId(requesterProfileId));
        Assert.Equal(requesterProfileId, connection.GetOtherParticipantId(addresseeProfileId));
    }
}
