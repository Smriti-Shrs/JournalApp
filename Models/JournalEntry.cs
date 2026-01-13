namespace JournalApp.Models;

public class JournalEntry
{
    public int Id { get; set; }

    // Basic text
    public string Title { get; set; } = "";
    public string Content { get; set; } = "";

    // One entry per calendar day
    public DateTime EntryDate { get; set; }

    // System-generated timestamps
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Mood tracking
    public string PrimaryMood { get; set; } = "";          // required
    public string SecondaryMoods { get; set; } = "";       // up to two moods, comma-separated

    // Tagging / categorisation (comma-separated tags)
    public string Tags { get; set; } = "";
}
