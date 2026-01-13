using JournalApp.Models;

namespace JournalApp.Services;

/// <summary>
/// Simple in-memory journal service. For the coursework you can later
/// replace the in-memory list with the SQLite DbContext (AppDbContext).
/// </summary>
public class JournalService
{
    // Static so all pages share the same in-memory data for now
    private static readonly List<JournalEntry> entries = new();

    public Task<List<JournalEntry>> GetEntriesAsync()
    {
        var ordered = entries
            .OrderByDescending(e => e.EntryDate)
            .ToList();

        return Task.FromResult(ordered);
    }

    public Task<JournalEntry?> GetEntryByIdAsync(int id)
    {
        return Task.FromResult(entries.FirstOrDefault(e => e.Id == id));
    }

    /// <summary>
    /// Get the entry for a specific calendar date (one entry per day).
    /// </summary>
    public Task<JournalEntry?> GetEntryByDateAsync(DateTime date)
    {
        var target = date.Date;
        var entry = entries.FirstOrDefault(e => e.EntryDate.Date == target);
        return Task.FromResult(entry);
    }

    public Task AddEntryAsync(JournalEntry entry)
    {
        entry.Id = entries.Count > 0 ? entries.Max(e => e.Id) + 1 : 1;
        entries.Add(entry);
        return Task.CompletedTask;
    }

    public Task UpdateEntryAsync(JournalEntry entry)
    {
        var existing = entries.FirstOrDefault(e => e.Id == entry.Id);
        if (existing != null)
        {
            existing.Title = entry.Title;
            existing.Content = entry.Content;
            existing.PrimaryMood = entry.PrimaryMood;
            existing.SecondaryMoods = entry.SecondaryMoods;
            existing.Tags = entry.Tags;
            existing.EntryDate = entry.EntryDate;
            existing.UpdatedAt = entry.UpdatedAt;
        }
        return Task.CompletedTask;
    }

    public Task DeleteEntryAsync(int id)
    {
        var entry = entries.FirstOrDefault(e => e.Id == id);
        if (entry != null)
            entries.Remove(entry);

        return Task.CompletedTask;
    }
}
