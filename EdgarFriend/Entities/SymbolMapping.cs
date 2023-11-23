using System.ComponentModel.DataAnnotations;

namespace EdgarFriend;

public class SymbolMapping
{
    [Key]
    public string Symbol { get; set; }
    public string Cik { get; set; }

    public SymbolMapping(string symbol, string cik)
    {
        this.Cik = cik;
        this.Symbol = symbol;
    }
}