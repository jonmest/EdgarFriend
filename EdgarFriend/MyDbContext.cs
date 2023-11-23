using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql;

namespace EdgarFriend;

public class UsGaapEntryDbConfiguration : IEntityTypeConfiguration<FundamentalEntry>
{
    public void Configure(EntityTypeBuilder<FundamentalEntry> builder)
    {
        builder.ToTable("FundamentalEntries");
        builder.HasKey(p => new { p.Cik, p.EntityName, p.Frame });
    }
}
public class MyDbContext : DbContext
{
    public DbSet<FundamentalEntry> FundamentalEntries { get; set; }
    public DbSet<SymbolMapping> SymbolMappings { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var host = Environment.GetEnvironmentVariable("DB_HOST");
        var database = Environment.GetEnvironmentVariable("DB_DATABASE");
        var username = Environment.GetEnvironmentVariable("DB_USER");
        var password = Environment.GetEnvironmentVariable("DB_PWD");
        optionsBuilder.UseNpgsql($"Host={host};Database={database};Username={username};Password={password};Timeout=500;MaxPoolSize=20;");
    }

    public async Task AddOrUpdateSymbolMappings(IEnumerable<SymbolMapping> mappings)
    {
        const string commandText = $@"
            INSERT INTO ""SymbolMappings""
                (""Cik"", ""Symbol"")
            VALUES
                (@Cik, @Symbol)
            ON CONFLICT (""Symbol"")
            DO NOTHING;";
        foreach (var entry in mappings)
        {
            await Database.ExecuteSqlRawAsync(commandText,
                new NpgsqlParameter("@Cik", entry.Cik),
                new NpgsqlParameter("@Symbol", entry.Symbol));
            
        }
    }
    
    public async Task AddOrUpdateFundamentalEntriesAsync(IEnumerable<FundamentalEntry> entries)
    {
        const string commandText = $@"
            INSERT INTO ""FundamentalEntries""
                (""Cik"", ""EntityName"", ""Label"", ""Unit"", ""Val"", ""Accn"", ""Form"", ""Fy"", ""Fp"", ""Filed"", ""Frame"", ""Start"", ""End"", ""PeriodType"", ""Symbol"")
            VALUES
                (@Cik, @EntityName, @Label, @Unit, @Val, @Accn, @Form, @Fy, @Fp, @Filed, @Frame, @Start, @End, @PeriodType, @Symbol)
            ON CONFLICT (""Cik"", ""Label"", ""Frame"")
            DO NOTHING;";

        foreach (var entry in entries)
        {
            await Database.ExecuteSqlRawAsync(commandText,
                new NpgsqlParameter("@Cik", entry.Cik),
                new NpgsqlParameter("@EntityName", entry.EntityName),
                new NpgsqlParameter("@Label", entry.Label),
                new NpgsqlParameter("@Symbol", entry.Symbol  ?? (object)DBNull.Value),
                new NpgsqlParameter("@Unit", entry.Unit),
                new NpgsqlParameter("@Val", entry.Val),
                new NpgsqlParameter("@Accn", entry.Accn),
                new NpgsqlParameter("@Form", entry.Form),
                new NpgsqlParameter("@Fy", entry.Fy),
                new NpgsqlParameter("@Fp", entry.Fp),
                new NpgsqlParameter("@Filed", entry.Filed),
                new NpgsqlParameter("@Frame", entry.Frame),
                new NpgsqlParameter("@Start", entry.Start ?? (object)DBNull.Value),
                new NpgsqlParameter("@End", entry.End),
                new NpgsqlParameter("@PeriodType", entry.PeriodType ?? (object)DBNull.Value));
        }
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<FundamentalEntry>()
            .HasKey(e => new { e.Cik, e.Frame, e.Label });
        modelBuilder.Entity<FundamentalEntry>()
            .HasIndex(entry => new { entry.Cik, entry.PeriodType, entry.Label, entry.Symbol })
            .IsUnique(false);
        
        modelBuilder.Entity<SymbolMapping>()
            .HasKey(e => new { e.Symbol });
        modelBuilder.Entity<SymbolMapping>()
            .HasIndex(m => new { m.Symbol, m.Cik });
    }
    
    public async Task MigrateAsync()
    {
        var pendingMigrations = this.Database.GetPendingMigrations().Any();
        if (pendingMigrations)
        {
            await Database.MigrateAsync();
        }
        else
        {
            var databaseExists = (this.GetService<IDatabaseCreator>() as RelationalDatabaseCreator).Exists();
            if (!databaseExists)
            {
                await Database.MigrateAsync();
            }
        }
    }
}