using EdgarFriend;
using System.Text.Json;



class Util {
    public static async Task ProcessJsonFileAsync(string filePath, CompanyInfo companyInfo, SemaphoreSlim semaphore)
    {
        await semaphore.WaitAsync();
        try
        {
            var options = new JsonSerializerOptions
            {
                Converters = { new RootObjectConverter(companyInfo.Dictionary) }
            };

            await using FileStream reader = File.OpenRead(filePath);
            RootObject? rootObject = await JsonSerializer.DeserializeAsync<RootObject>(reader, options);

            if (rootObject == null) return;

            var entries = rootObject.Facts.UsGaap;
            entries.Sort(new UsGaapEntryPriorityComparer());

            var deduplicatedEntries = entries
                .Where(entry => entry.Frame != null)
                .Distinct(new UsGaapEntryComparer())
                .ToList();

            var estimatedEntries = QuarterFourEstimator.Estimate(deduplicatedEntries);

            await using (var db = new MyDbContext())
            {
                await using (var transaction = await db.Database.BeginTransactionAsync())
                {
                    await db.AddOrUpdateFundamentalEntriesAsync(deduplicatedEntries);
                    await db.AddOrUpdateFundamentalEntriesAsync(estimatedEntries);
                    await db.SaveChangesAsync();
                    await transaction.CommitAsync();
                }
            }
        }
        finally
        {
            semaphore.Release();
        }
    }
}