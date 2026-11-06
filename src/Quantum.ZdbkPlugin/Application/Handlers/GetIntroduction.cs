using NOF.Contract;

namespace Quantum.OfficialPlugins.Zdbk;

public sealed class GetIntroduction(AcademicSession session) : ZdbkRpcService.GetIntroduction
{
    public override async Task<Result<string>> HandleAsync(
        AcademicCourseRequest request, Context context, CancellationToken cancellationToken)
    {
        var data = await session.ReadAsync(new AcademicQuery
        {
            Resource = "zdbk-introduction",
            Course = request.Course
        }, cancellationToken);
        return data.Text("text");
    }
}
