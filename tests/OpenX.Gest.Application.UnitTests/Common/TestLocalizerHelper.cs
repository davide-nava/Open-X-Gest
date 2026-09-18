using Microsoft.Extensions.Localization;
using Moq;
using OpenX.Gest.Application.Resources;

namespace OpenX.Gest.Application.UnitTests.Common;

/// <summary>
/// Helper per la configurazione dei mock di localizzazione per i test di unità CQRS.
/// </summary>
public static class TestLocalizerHelper
{
    public static IStringLocalizer<ValidationMessages> CreateMockLocalizer()
    {
        var mock = new Mock<IStringLocalizer<ValidationMessages>>();
        mock.Setup(l => l[It.IsAny<string>()])
            .Returns((string name) => new LocalizedString(name, name));
        return mock.Object;
    }
}
