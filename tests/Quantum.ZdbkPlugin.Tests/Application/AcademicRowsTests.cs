using Quantum.OfficialPlugins.Zdbk;
using System.Globalization;
using System.Text.Json;

namespace Quantum.ZdbkPlugin.Tests.Application;

public sealed class AcademicRowsTests
{
    [Fact]
    public void AlignsScheduleAndRoomsAcrossHtmlBreakVariants()
    {
        var row = JsonSerializer.SerializeToElement(new
        {
            xkkh = "fictional-section",
            sksj = "<span>周一第1,2节</span><BR/>周三第3,4节<br />周五第5,6节",
            skdd = "Room&nbsp;A<br><br/>Room C",
            jsxm = "Teacher A<br/>Teacher B",
            rs = "4/30",
            yxrs = "2~7"
        });
        var section = AcademicRows.Section(row, null);
        Assert.Equal(["周一第1,2节", "周三第3,4节", "周五第5,6节"], section.Meetings.Select(m => m.Time));
        Assert.Equal(["Room\u00a0A", "", "Room C"], section.Meetings.Select(m => m.Location));
        Assert.Equal(["Teacher A", "Teacher B"], section.Instructors);
        Assert.Equal(4, section.AvailableSeats);
        Assert.Equal(30, section.Capacity);
        Assert.Equal(2, section.MajorWaitingCount);
        Assert.Equal(7, section.TotalWaitingCount);
    }

    [Fact]
    public void MissingOptionalFieldsDoNotInventScheduleOrSeats()
    {
        var section = AcademicRows.Section(JsonSerializer.SerializeToElement(new
        {
            xkkh = "fictional"
        }), null);
        Assert.Empty(section.Meetings);
        Assert.Empty(section.Instructors);
        Assert.Equal(0, section.AvailableSeats);
        Assert.Equal(0, section.Capacity);
    }

    [Fact]
    public void CreditsUseTheUpstreamDecimalFormatRegardlessOfCurrentCulture()
    {
        var original = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("de-DE");
            Assert.Equal(3.5m, AcademicRows.Credits("3.5"));
        }
        finally { CultureInfo.CurrentCulture = original; }
    }
}
