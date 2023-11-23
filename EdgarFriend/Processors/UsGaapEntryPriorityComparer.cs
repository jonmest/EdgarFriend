namespace EdgarFriend;

public class UsGaapEntryPriorityComparer : IComparer<FundamentalEntry>
{
    
    
    public int Compare(FundamentalEntry? x, FundamentalEntry? y)
    {
        var compareDate = string.Compare(y?.Filed, x?.Filed, StringComparison.Ordinal);  // Descending order of filing date
        if (compareDate != 0) return compareDate;

        var compareFrame = (y?.Frame != null).CompareTo(x?.Frame != null);  // Entries with defined Frame come first
        if (compareFrame != 0) return compareFrame;

        var xIsAmended = x?.Form is "10-K/A" or "10-Q/A";
        var yIsAmended = y?.Form is "10-K/A" or "10-Q/A";
        var compareForm = yIsAmended.CompareTo(xIsAmended);  // Amended forms come first
        if (compareForm != 0) return compareForm;

        var compareStartDates = x is { Start: not null } && y.Start.HasValue 
            ? y.Start.Value.CompareTo(x.Start.Value)  // Descending order of start date
            : 0; 
        return compareStartDates != 0 ? compareStartDates :
            // Continue with any additional comparisons if necessary
            0;
    }
}
