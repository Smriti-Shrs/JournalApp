using JournalApp.Models;

namespace JournalApp.Services;

public class JournalService
{
    private readonly List<JournalEntry> entries = new();

    public Task<List<JournalEntry>> GetEntriesAsync()
    {
        return Task.FromResult(entries.OrderByDescending(e => e.EntryDate).ToList());
    }

    public Task<JournalEntry?> GetEntryByIdAsync(int id)
    {
        return Task.FromResult(entries.FirstOrDefault(e => e.Id == id));
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
