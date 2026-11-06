using NOF.Contract;

namespace Quantum.OfficialPlugins.Zdbk;

public sealed class ListCourses(AcademicSession session) : ZdbkRpcService.ListCourses
{
    public override async Task<Result<AcademicCourse[]>> HandleAsync(
        ListCoursesRequest request, Context context, CancellationToken cancellationToken)
    {
        var data = await session.ReadAsync(new AcademicQuery
        {
            Resource = "zdbk-courses",
            Category = request.Category,
            Start = request.Start,
            End = request.End
        }, cancellationToken);
        return data.GetProperty("rows").EnumerateArray().Select(row =>
        {
            var info = row.Text("kcxx").Split('~');
            return new AcademicCourse
            {
                Id = row.Text("kcdm"),
                Code = row.Text("xkkh"),
                Name = row.Text("kcmc"),
                Credits = AcademicRows.Credits(info.Length > 1 ? info[1] : ""),
                WeekTime = info.Length > 2 ? info[2] : "",
                Department = row.Text("kkxy"),
                Property = row.Text("kcxz"),
                Status = row.Text("kcxzzt") == "1" ? "Selected" : "NotSelected"
            };
        }).DistinctBy(course => course.Code).ToArray();
    }
}
