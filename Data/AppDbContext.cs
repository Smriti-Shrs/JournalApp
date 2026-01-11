using Microsoft.EntityFrameworkCore;
using JournalApp.Models;

namespace JournalApp.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<JournalEntry> JournalEntries { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseSqlite("Data Source=journal.db");
        }
    }
}
