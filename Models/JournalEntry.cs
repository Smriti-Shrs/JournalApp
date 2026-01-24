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

    // Markdown support
    public string MarkdownContent { get; set; } = "";

    // Word count for analytics
    public int WordCount { get; set; }

    /// <summary>
    /// Updates the word count based on content.
    /// </summary>
    public void UpdateWordCount()
    {
        WordCount = string.IsNullOrWhiteSpace(Content) ? 0 : 
            Content.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).Length;
    }

    /// <summary>
    /// Gets tags as a list.
    /// </summary>
    public List<string> GetTagsList()
    {
        if (string.IsNullOrWhiteSpace(Tags))
            return new List<string>();
        
        return Tags.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(t => t.Trim())
            .Where(t => !string.IsNullOrEmpty(t))
            .ToList();
    }

    /// <summary>
    /// Gets secondary moods as a list.
    /// </summary>
    public List<string> GetSecondaryMoodsList()
    {
        if (string.IsNullOrWhiteSpace(SecondaryMoods))
            return new List<string>();
        
        return SecondaryMoods.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(m => m.Trim())
            .Where(m => !string.IsNullOrEmpty(m))
            .ToList();
    }
}
