namespace GtaConnect.Application.Common.Interfaces;

// Abstrai "quem está online agora" pra Application. Sem persistência — presença é estado
// efêmero de runtime, não dado de negócio (reinicia zerado se a API reiniciar, e tudo bem).
public interface IPresenceTracker
{
    void MarkOnline(Guid userId);

    void MarkOffline(Guid userId);

    bool IsOnline(Guid userId);
}
