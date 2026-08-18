using GtaConnect.Domain.Entities;
using GtaConnect.Domain.Enums;

namespace GtaConnect.Domain.Tests.Entities;

public class ReportTests
{
    [Fact]
    public void Create_ComDadosValidos_CriaReportComDetailsAparado()
    {
        var reporterProfileId = Guid.NewGuid();
        var reportedProfileId = Guid.NewGuid();

        var report = Report.Create(reporterProfileId, reportedProfileId, ReportReason.Toxicidade, "  ficou xingando o time todo  ");

        Assert.Equal(reporterProfileId, report.ReporterProfileId);
        Assert.Equal(reportedProfileId, report.ReportedProfileId);
        Assert.Equal(ReportReason.Toxicidade, report.Reason);
        Assert.Equal("ficou xingando o time todo", report.Details);
    }

    [Fact]
    public void Create_SemDetails_MantemDetailsNull()
    {
        var report = Report.Create(Guid.NewGuid(), Guid.NewGuid(), ReportReason.Spam, null);

        Assert.Null(report.Details);
    }

    [Fact]
    public void Create_ComMesmoPerfilNosDoisLados_LancaArgumentException()
    {
        var profileId = Guid.NewGuid();

        Assert.Throws<ArgumentException>(() => Report.Create(profileId, profileId, ReportReason.Outro, null));
    }

    [Fact]
    public void Create_ComMotivoInvalido_LancaArgumentException()
    {
        var invalidReason = (ReportReason)999;

        Assert.Throws<ArgumentException>(() => Report.Create(Guid.NewGuid(), Guid.NewGuid(), invalidReason, null));
    }

    [Fact]
    public void Create_ComDetailsMuitoLongo_LancaArgumentException()
    {
        var tooLong = new string('a', 501);

        Assert.Throws<ArgumentException>(() => Report.Create(Guid.NewGuid(), Guid.NewGuid(), ReportReason.Outro, tooLong));
    }
}
