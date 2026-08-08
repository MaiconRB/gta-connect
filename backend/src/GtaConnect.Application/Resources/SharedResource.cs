namespace GtaConnect.Application.Resources;

/// <summary>
/// Classe marcadora sem lógica — existe só para dar um "endereço" de tipo para
/// IStringLocalizer&lt;SharedResource&gt;. O ASP.NET Core resolve o .resx pelo
/// assembly deste tipo, então tanto Application quanto Api conseguem injetar
/// IStringLocalizer&lt;SharedResource&gt; e enxergar os mesmos recursos.
/// </summary>
public sealed class SharedResource;
