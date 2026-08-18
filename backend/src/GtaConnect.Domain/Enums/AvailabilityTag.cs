namespace GtaConnect.Domain.Enums;

/// <summary>
/// Janelas de horário em que o jogador costuma jogar, multi-seleção via bitmask.
/// Mesmo padrão de PlaystyleTag — lista curada e fixa, [Flags] em vez de tabela
/// de junção (ver justificativa completa em PlaystyleTag.cs).
/// </summary>
[Flags]
public enum AvailabilityTag
{
    None = 0,
    Manha = 1 << 0,
    Tarde = 1 << 1,
    Noite = 1 << 2,
    Madrugada = 1 << 3,
    FimDeSemana = 1 << 4,

    All = Manha | Tarde | Noite | Madrugada | FimDeSemana,
}
