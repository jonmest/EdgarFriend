using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace EdgarFriend;

public class RootObject
{
    public string? Symbol { get; set; }
    public string? Cik { get; set; }
    public string? EntityName  { get; set; }
    public Facts? Facts  { get; set; }
    
    public override string ToString()
    {
        return JsonConvert.SerializeObject(this);
    }
}

public partial class Facts
{
    public List<FundamentalEntry>? Dei { get; set; }

    public List<FundamentalEntry>? UsGaap { get; set; }
    
    public override string ToString()
    {
        return JsonConvert.SerializeObject(this);
    }
}

[Table("FundamentalEntries")]

public class FundamentalEntry
{
    public string? Symbol { get; set; }
    [Key]
    public string? Cik { get; set; }
    public string? EntityName  { get; set; }
    [Key]
    public string? Label { get; set; }
    public string? Unit { get; set; }
    public double Val { get; set; }
    public string? Accn { get; set; }
    public string? Form { get; set; }
    public int? Fy { get; set; }
    public string? Fp { get; set; }
    public string? Filed { get; set; }
    [Key]
    public string? Frame { get; set; }
    public DateOnly? Start { get; set; }
    public DateOnly End { get; set; }

    public string? PeriodType { get; set; }
    
    public override string ToString()
    {
        return JsonConvert.SerializeObject(this);
    }
    
}