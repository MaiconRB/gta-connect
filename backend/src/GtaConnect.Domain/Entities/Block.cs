namespace GtaConnect.Domain.Entities;

// Direcional no registro (só quem bloqueou pode desbloquear), mas o EFEITO de "existe
// bloqueio entre A e B" é sempre checado nos dois sentidos pela Application — ver
// IBlockRepository.ExistsEitherDirectionAsync/GetBlockedOrBlockingProfileIdsAsync.
public class Block
{
    public Guid Id { get; private set; }

    public Guid BlockerProfileId { get; private set; }

    public Guid BlockedProfileId { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    private Block()
    {
    }

    public static Block Create(Guid blockerProfileId, Guid blockedProfileId)
    {
        if (blockerProfileId == Guid.Empty || blockedProfileId == Guid.Empty)
        {
            throw new ArgumentException("Os perfis do bloqueio não podem ser vazios.");
        }

        if (blockerProfileId == blockedProfileId)
        {
            throw new ArgumentException("Não é possível bloquear a si mesmo.");
        }

        return new Block
        {
            Id = Guid.NewGuid(),
            BlockerProfileId = blockerProfileId,
            BlockedProfileId = blockedProfileId,
            CreatedAtUtc = DateTime.UtcNow,
        };
    }
}
