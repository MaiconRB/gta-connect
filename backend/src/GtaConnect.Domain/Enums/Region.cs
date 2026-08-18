namespace GtaConnect.Domain.Enums;

/// <summary>
/// Macro-região do Brasil onde o jogador está. Seleção única (não é [Flags]) — cada
/// jogador mora numa região só. Campo opcional no perfil (PlayerProfile.Region é nullable).
/// </summary>
public enum Region
{
    Norte = 1,
    Nordeste = 2,
    CentroOeste = 3,
    Sudeste = 4,
    Sul = 5,
}
