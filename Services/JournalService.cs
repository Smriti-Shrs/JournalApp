using JournalApp.Data;
using JournalApp.Models;
using Microsoft.EntityFrameworkCore;

namespace JournalApp.Services;

/// <summary>
/// Journal service backed by local SQLite (AppDbContext).
/// </summary>
public class JournalService
{
    private static bool _databaseInitialized;

    private static void EnsureDatabase()
    {
        if (_databaseInitialized)
            return;

        using var db = new AppDbContext();
        db.Database.EnsureCreated();
        _databaseInitialized = true;
    }

    public async Task<List<JournalEntry>> GetEntriesAsync()
    {
        EnsureDatabase();
        await using var db = new AppDbContext();

        return await db.JournalEntries
            .OrderByDescending(e => e.EntryDate)
            .ToListAsync();
    }

    public async Task<JournalEntry?> GetEntryByIdAsync(int id)
    {
        EnsureDatabase();
        await using var db = new AppDbContext();
        return await db.JournalEntries.FirstOrDefaultAsync(e => e.Id == id);
    }

    /// <summary>
    /// Get the entry for a specific calendar date (one entry per day).
    /// </summary>
    public async Task<JournalEntry?> GetEntryByDateAsync(DateTime date)
    {
        EnsureDatabase();
        await using var db = new AppDbContext();

        var target = date.Date;
        return await db.JournalEntries.FirstOrDefaultAsync(e => e.EntryDate.Date == target);
    }

    public async Task AddEntryAsync(JournalEntry entry)
    {
        EnsureDatabase();
        await using var db = new AppDbContext();

        entry.CreatedAt = DateTime.Now;
        entry.UpdatedAt = entry.CreatedAt;

        db.JournalEntries.Add(entry);
        await db.SaveChangesAsync();
    }

    public async Task UpdateEntryAsync(JournalEntry entry)
    {
        EnsureDatabase();
        await using var db = new AppDbContext();

        var existing = await db.JournalEntries.FirstOrDefaultAsync(e => e.Id == entry.Id);
        if (existing != null)
        {
            existing.Title = entry.Title;
            existing.Content = entry.Content;
            existing.PrimaryMood = entry.PrimaryMood;
            existing.SecondaryMoods = entry.SecondaryMoods;
            existing.Tags = entry.Tags;
            existing.EntryDate = entry.EntryDate;
            existing.UpdatedAt = DateTime.Now;

            await db.SaveChangesAsync();
        }
    }

    public async Task DeleteEntryAsync(int id)
    {
        EnsureDatabase();
        await using var db = new AppDbContext();

        var entry = await db.JournalEntries.FirstOrDefaultAsync(e => e.Id == id);
        if (entry != null)
        {
            db.JournalEntries.Remove(entry);
            await db.SaveChangesAsync();
        }
    }
}
