using EdgarFriend;
using System.Diagnostics;
using System.IO.Compression;
using System.Text.Json;


class Program
{
    private static readonly SemaphoreSlim Semaphore = new SemaphoreSlim(int.Parse(Environment.GetEnvironmentVariable("MAX_CONCURRENT") ?? "2"));
    private const string SecUrl = "http://www.sec.gov/Archives/edgar/daily-index/xbrl/companyfacts.zip";
    private const string CompanyTickersUrl = "https://www.sec.gov/files/company_tickers.json";

    private static async Task<string> DownloadAndExtractZipAsync(string url)
    {
        string tempZipFilePath = Path.GetTempFileName();
        string unzippedDirectoryPath = Path.Combine(Path.GetTempPath(), "unzipped");

        using var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", Environment.GetEnvironmentVariable("USER_AGENT"));
        httpClient.DefaultRequestHeaders.AcceptEncoding.ParseAdd("gzip, deflate");
        httpClient.DefaultRequestHeaders.Host = "www.sec.gov";
        httpClient.Timeout = TimeSpan.FromMinutes(10);

        using var response = await httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();

        await using (var fileStream = new FileStream(tempZipFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
        {
            await response.Content.CopyToAsync(fileStream);
        }

        if (Directory.Exists(unzippedDirectoryPath))
        {
            Directory.Delete(unzippedDirectoryPath, true);
        }

        ZipFile.ExtractToDirectory(tempZipFilePath, unzippedDirectoryPath);
        File.Delete(tempZipFilePath);
        return unzippedDirectoryPath;
    }

    private static async Task<CompanyInfo?> GetSymbolsMap()
    {
        using var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", Environment.GetEnvironmentVariable("USER_AGENT"));
        httpClient.DefaultRequestHeaders.Host = "www.sec.gov";
        httpClient.Timeout = TimeSpan.FromMinutes(10);

        using var response = await httpClient.GetAsync(CompanyTickersUrl);
        response.EnsureSuccessStatusCode();
        await using var stream = response.Content.ReadAsStream();

        var options = new JsonSerializerOptions();
        options.Converters.Add(new CompanyInfoConverter());
        return await JsonSerializer.DeserializeAsync<CompanyInfo>(stream, options);
    }


    static async Task Main(string[] args)
    {
        foreach (var dbConfigVariable in new[] { "USER_AGENT", "DB_HOST", "DB_DATABASE", "DB_USER", "DB_PWD" })
        {
            if (Environment.GetEnvironmentVariable(dbConfigVariable) == null)
            {
                throw new ArgumentException($"Please set the {dbConfigVariable} environment variable.");
            }
        }


        await using (var db = new MyDbContext()) await db.MigrateAsync();

        CompanyInfo? companyInfo = await GetSymbolsMap();
        if (companyInfo == null) return;

        await using (var db = new MyDbContext())
        {
            await using var transaction = await db.Database.BeginTransactionAsync();
            await db.AddOrUpdateSymbolMappings(companyInfo.Mappings);
            await db.SaveChangesAsync();
            await transaction.CommitAsync();
        }

        var stopwatch = new Stopwatch();
        stopwatch.Start();

        string directoryPath = await DownloadAndExtractZipAsync(SecUrl);
        var jsonFiles = Directory.GetFiles(directoryPath, "*.json");

        var tasks = jsonFiles.Select(filePath => Util.ProcessJsonFileAsync(filePath, companyInfo, Semaphore));
        await Task.WhenAll(tasks);

        stopwatch.Stop();
        Console.WriteLine($"Execution Time in total for {jsonFiles.Length} files: {stopwatch.ElapsedMilliseconds} ms");
    }
}
