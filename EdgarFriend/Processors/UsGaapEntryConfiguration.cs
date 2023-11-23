using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EdgarFriend;

public class UsGaapEntryConfiguration : IEntityTypeConfiguration<FundamentalEntry>
{
    public void Configure(EntityTypeBuilder<FundamentalEntry> builder)
    {

        builder
            .HasIndex(entry => new { entry.Cik, entry.PeriodType, entry.Label })
            .IsUnique(false);
    }
}