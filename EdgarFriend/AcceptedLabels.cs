namespace EdgarFriend;

public static class AcceptedLabels
{
    public static bool Contains(string? label)
    {
        if (label == null) return false;
        return FinancialMetrics.Contains(label);
    }
    
    private static readonly HashSet<string> FinancialMetrics = new HashSet<string>()
    {
        "Assets",
        "AssetsCurrent",
        "NoncurrentAssets",
        "Liabilities",
        "LiabilitiesCurrent",
        "LiabilitiesNoncurrent",
        "StockholdersEquity",
        "Capital",
        "EarningsPerShareBasic",
        "EarningsPerShareDiluted",
        "NetIncomeLoss",
        "NetIncomeLossAvailableToCommonStockholdersBasic",
        "NetIncomeLossAvailableToCommonStockholdersDiluted",
        "GrossProfit",
        "OperatingIncomeLoss",
        "ProfitLoss",
        "CommonStockDividendsPerShareCashPaid",
        "CommonStockDividendsPerShareDeclared",
        "DividendsPayableCurrent",
        "DividendsPayableCurrentAndNoncurrent",
        "CommonStockSharesOutstanding",
        "CommonStockSharesAuthorized",
        "CommonStockSharesIssued",
        "PreferredStockSharesOutstanding",
        "PreferredStockSharesAuthorized",
        "PreferredStockSharesIssued",
        "Revenues",
        "OperatingExpenses",
        "ResearchAndDevelopmentExpense",
        "DebtCurrent",
        "LongTermDebt",
        "Goodwill",
        "ShareBasedCompensation",
        "PreferredStockValue",
        "EntityPublicFloat"
    };
    
}