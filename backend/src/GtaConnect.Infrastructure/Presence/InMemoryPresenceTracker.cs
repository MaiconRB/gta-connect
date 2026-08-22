using System.Collections.Concurrent;
using GtaConnect.Application.Common.Interfaces;

namespace GtaConnect.Infrastructure.Presence;

// Um usuário pode ter mais de uma conexão SignalR simultânea (várias abas/dispositivos) —
// por isso conta conexões ativas por usuário, não um bool solto. Só fica "offline" quando
// a última cai. Registrado como Singleton (não Scoped) — precisa sobreviver entre requests.
public class InMemoryPresenceTracker : IPresenceTracker
{
    private readonly ConcurrentDictionary<Guid, int> _connectionCountsByUserId = new();

    public void MarkOnline(Guid userId)
    {
        _connectionCountsByUserId.AddOrUpdate(userId, 1, (_, count) => count + 1);
    }

    public void MarkOffline(Guid userId)
    {
        _connectionCountsByUserId.AddOrUpdate(userId, 0, (_, count) => Math.Max(0, count - 1));

        if (_connectionCountsByUserId.TryGetValue(userId, out var remaining) && remaining <= 0)
        {
            _connectionCountsByUserId.TryRemove(userId, out _);
        }
    }

    public bool IsOnline(Guid userId) => _connectionCountsByUserId.ContainsKey(userId);
}
