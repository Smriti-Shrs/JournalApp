using Microsoft.EntityFrameworkCore;
using JournalApp.Models;

namespace JournalApp.Data;

public class AppDbContext : DbContext
{
    public DbSet<JournalEntry> JournalEntries { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        var dbPath = Path.Combine(FileSystem.AppDataDirectory, "journal.db");
        options.UseSqlite($"Data Source={dbPath}");
    }
}
