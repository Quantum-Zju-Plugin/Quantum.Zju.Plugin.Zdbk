using HtmlAgilityPack;
using System.Globalization;
using System.Text.Json;

namespace Quantum.OfficialPlugins.Zdbk;

internal static class AcademicRows
{
    internal static string Text(this JsonElement row, string name)
        => row.TryGetProperty(name, out var field) ? field.ToString() : "";

    internal static decimal Credits(string text)
        => decimal.TryParse(text, NumberStyles.Number, CultureInfo.InvariantCulture, out var credits) ? credits : 0;

    internal static AcademicSection Section(JsonElement row, AcademicCourse? course)
    {
        var (available, capacity) = Pair(row.Text("rs"), '/');
        var (major, total) = Pair(row.Text("yxrs"), '~');
        var times = Split(row.Text("sksj"));
        var locations = Split(row.Text("skdd"));
        return new AcademicSection
        {
            Id = row.Text("xkkh"),
            Course = course,
            Instructors = Split(row.Text("jsxm")),
            Meetings = times.Select((time, index) => new Meeting
            {
                Time = time,
                Location = index < locations.Length ? locations[index] : ""
            }).ToArray(),
            Exam = row.Text(course is null ? "kssj" : "vkssj"),
            Semesters = row.Text("xxq"),
            IsInternational = row.Text("gjhkc") == "是",
            TeachingForm = row.Text("jxfs"),
            TeachingMethod = row.Text("skxs"),
            AvailableSeats = available,
            Capacity = capacity,
            MajorWaitingCount = major,
            TotalWaitingCount = total
        };
    }

    private static string[] Split(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return [];
        }
        var document = new HtmlDocument();
        document.LoadHtml(value);
        foreach (var lineBreak in document.DocumentNode.Descendants("br").ToArray())
        {
            lineBreak.ParentNode.ReplaceChild(document.CreateTextNode("\n"), lineBreak);
        }
        return HtmlEntity.DeEntitize(document.DocumentNode.InnerText).Split('\n', StringSplitOptions.TrimEntries);
    }

    private static (int, int) Pair(string value, char separator)
    {
        var parts = value.Split(separator);
        return parts.Length == 2
            && int.TryParse(parts[0], CultureInfo.InvariantCulture, out var first)
            && int.TryParse(parts[1], CultureInfo.InvariantCulture, out var second)
            ? (first, second) : (0, 0);
    }
}
