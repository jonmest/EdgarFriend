namespace EdgarFriend;

public class UsGaapEntryComparer : IEqualityComparer<FundamentalEntry>
{
    public bool Equals(FundamentalEntry? x, FundamentalEntry? y)
    {
        return x?.Cik == y?.Cik
               && x?.EntityName == y?.EntityName
               && x?.Label == y?.Label
               && x?.Start == y?.Start
               && x?.End == y?.End;
    }

    public int GetHashCode(FundamentalEntry obj)
    {
        return HashCode.Combine(obj.Cik, obj.EntityName, obj.Label, obj.Start, obj.End);
    }
}