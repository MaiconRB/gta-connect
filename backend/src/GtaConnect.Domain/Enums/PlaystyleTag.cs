namespace GtaConnect.Domain.Enums;

/// <summary>
/// Tags de estilo de jogo, multi-seleção via bitmask. Lista curada e fixa em código
/// (não editável em runtime) — por isso um [Flags] enum, e não uma tabela de junção
/// many-to-many, que seria complexidade sem ganho real neste porte de projeto.
/// Gatilho pra migrar pra tabela de junção: lista passar de ~20 tags, ou precisar
/// de metadados por tag (peso, ordem, tradução server-side).
/// </summary>
[Flags]
public enum PlaystyleTag
{
    None = 0,
    MundoAbertoCalmo = 1 << 0,
    GrindDeHeist = 1 << 1,
    Corrida = 1 << 2,
    RolePlay = 1 << 3,
    FreemodeSocial = 1 << 4,
    CampanhaHistoria = 1 << 5,
    PvpCompetitivo = 1 << 6,
    NegociosEconomia = 1 << 7,

    All = MundoAbertoCalmo | GrindDeHeist | Corrida | RolePlay | FreemodeSocial | CampanhaHistoria | PvpCompetitivo | NegociosEconomia,
}
