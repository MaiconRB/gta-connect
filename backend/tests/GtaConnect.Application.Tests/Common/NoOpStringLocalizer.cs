using GtaConnect.Application.Resources;
using Microsoft.Extensions.Localization;
using Moq;

namespace GtaConnect.Application.Tests.Common;

/// <summary>
/// Fake de IStringLocalizer para testes: devolve a própria chave como "texto",
/// já que os testes de validators/AuthService verificam PropertyName/IsValid,
/// nunca o texto final da mensagem. Evita depender dos .resx reais nos testes.
/// </summary>
public static class NoOpStringLocalizer
{
    public static IStringLocalizer<SharedResource> Create()
    {
        var mock = new Mock<IStringLocalizer<SharedResource>>();

        mock.Setup(l => l[It.IsAny<string>()])
            .Returns((string name) => new LocalizedString(name, name));

        mock.Setup(l => l[It.IsAny<string>(), It.IsAny<object[]>()])
            .Returns((string name, object[] args) => new LocalizedString(name, name));

        return mock.Object;
    }
}
