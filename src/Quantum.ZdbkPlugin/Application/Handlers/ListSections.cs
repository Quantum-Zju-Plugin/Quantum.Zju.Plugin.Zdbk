using NOF.Contract;

namespace Quantum.OfficialPlugins.Zdbk;

public sealed class ListSections(AcademicSession session) : ZdbkRpcService.ListSections
{
    public override async Task<Result<AcademicSection[]>> HandleAsync(
        AcademicCourseRequest request, Context context, CancellationToken cancellationToken)
    {
        var data = await session.ReadAsync(new AcademicQuery
        {
            Resource = "zdbk-sections",
            Course = request.Course
        }, cancellationToken);
        return data.GetProperty("rows").EnumerateArray().Select(row => AcademicRows.Section(row, null))
            .DistinctBy(section => section.Id).ToArray();
    }
}
