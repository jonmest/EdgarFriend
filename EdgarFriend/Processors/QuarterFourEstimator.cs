using System.Diagnostics;

namespace EdgarFriend;

public static class QuarterFourEstimator
{
    private static int GetYear(string? frame)
    {
        Debug.Assert(frame != null, nameof(frame) + " != null");
        if (frame.Length < 6 || !frame.StartsWith("CY"))
            throw new InvalidOperationException("The frame format is invalid.");
        if (int.TryParse(frame.AsSpan(2, 4), out var year))
        {
            return year;
        }
        throw new InvalidOperationException("The frame format is invalid.");
    }

    public static IEnumerable<FundamentalEntry> Estimate(IEnumerable<FundamentalEntry> entries)
    {
        var fundamentalEntries = entries as FundamentalEntry[] ?? entries.ToArray();
        var entriesByYearAndLabel = fundamentalEntries.ToLookup(
            entry => (Year: GetYear(entry.Frame), Label: entry.Label)
        );

        return (from grouping in entriesByYearAndLabel
            let year = grouping.Key.Year
            let label = grouping.Key.Label
            let labelEntries = grouping.ToList()
            let quarterly = labelEntries.Where(e => e.PeriodType == "Q")
            let quarterlySum = quarterly.Sum(e => e.Val)
            let yearly = labelEntries.FirstOrDefault(e => e.PeriodType == "A")
            where yearly != null && quarterly.Count() == 3
            select new FundamentalEntry
            {
                Symbol = yearly.Symbol,
                Label = yearly.Label,
                Accn = yearly.Accn,
                Cik = yearly.Cik,
                Start = yearly.Start, //Todo
                End = yearly.End,
                EntityName = yearly.EntityName,
                Filed = yearly.Filed,
                Form = yearly.Form,
                Fp = "Q4",
                Frame = $"CY{year}Q4",
                Fy = yearly.Fy,
                Val = yearly.Val - quarterlySum,
                Unit = yearly.Unit,
                PeriodType = "Q"
            }).ToList();
    }
}