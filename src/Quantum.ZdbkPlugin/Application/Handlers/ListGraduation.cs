using NOF.Contract;

namespace Quantum.OfficialPlugins.Zdbk;

public sealed class ListGraduation(AcademicSession session) : ZdbkRpcService.ListGraduation
{
    public override async Task<Result<AcademicCourse[]>> HandleAsync(
        AcademicListRequest request, Context context, CancellationToken cancellationToken)
    {
        var data = await session.ReadAsync(new AcademicQuery
        {
            Resource = "zdbk-graduation"
        }, cancellationToken);
        var term = $"T({data.Text("year")}-{data.Text("semester")})-";
        return data.GetProperty("rows").EnumerateArray().Where(row =>
            row.Text("KCDM").Length > 0 && row.Text("JDMC") is not ("" or "课程名称")
            && row.TryGetProperty("KCXF", out _)).Select(row => new AcademicCourse
            {
                Id = row.Text("KCDM"),
                Code = term + row.Text("KCDM"),
                Name = row.Text("JDMC"),
                Credits = AcademicRows.Credits(row.Text("KCXF")),
                WeekTime = "",
                Department = "",
                Property = "",
                Status = row.Text("SFTG") switch
                {
                    "通过" => "Passed",
                    "不通过" => "Failed",
                    "" when row.Text("KCBZ") == "在修" => "Selected",
                    "" => "NotSelected",
                    _ => "Unknown"
                }
            }).DistinctBy(course => course.Id).ToArray();
    }
}
