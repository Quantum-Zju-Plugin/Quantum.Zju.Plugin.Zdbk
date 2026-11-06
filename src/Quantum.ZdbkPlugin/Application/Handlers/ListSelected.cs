using NOF.Contract;

namespace Quantum.OfficialPlugins.Zdbk;

public sealed class ListSelected(AcademicSession session) : ZdbkRpcService.ListSelected
{
    public override async Task<Result<AcademicSection[]>> HandleAsync(
        AcademicListRequest request, Context context, CancellationToken cancellationToken)
    {
        var data = await session.ReadAsync(new AcademicQuery
        {
            Resource = "zdbk-selected"
        }, cancellationToken);
        return data.GetProperty("rows").EnumerateArray().Select(row => AcademicRows.Section(row, new AcademicCourse
        {
            Id = row.Text("kcdm"),
            Code = row.Text("xkkh"),
            Name = row.Text("kcmc"),
            Credits = AcademicRows.Credits(row.Text("xf")),
            WeekTime = row.Text("zxs"),
            Department = row.Text("kkxy"),
            Property = row.Text("kcxz"),
            Status = "Selected"
        })).DistinctBy(section => section.Id).ToArray();
    }
}
