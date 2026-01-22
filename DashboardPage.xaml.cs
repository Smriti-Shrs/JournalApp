using System.Collections.ObjectModel;
using JournalApp.Models;
using JournalApp.Services;

namespace JournalApp;

public partial class DashboardPage : ContentPage
{
    private readonly JournalService _journalService = new();
    private readonly ObservableCollection<JournalEntry> _recentEntries = new();

    public static readonly string[] PositiveMoods =
    {
        "Happy", "Excited", "Relaxed", "Grateful", "Confident"
    };

    public static readonly string[] NeutralMoods =
    {
        "Calm", "Thoughtful", "Curious", "Nostalgic", "Bored"
    };

    public static readonly string[] NegativeMoods =
    {
        "Sad", "Angry", "Stressed", "Lonely", "Anxious"
    };

    public DashboardPage()
    {
        InitializeComponent();

        RecentEntriesCollectionView.ItemsSource = _recentEntries;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        var entries = await _journalService.GetEntriesAsync();
        var ordered = entries.OrderByDescending(e => e.EntryDate.Date).ToList();

        // Recent entries
        _recentEntries.Clear();
        foreach (var entry in ordered.Take(5))
            _recentEntries.Add(entry);

        // Simple stats
        TotalEntriesLabel.Text = ordered.Count.ToString();

        if (ordered.Count == 0)
        {
            CurrentStreakLabel.Text = "0 days";
            LongestStreakLabel.Text = "0 days";
            AverageWordsLabel.Text = "0";

            PositivePercentLabel.Text = "0%";
            PositiveCountLabel.Text = "(0 entries)";
            NeutralPercentLabel.Text = "0%";
            NeutralCountLabel.Text = "(0 entries)";
            NegativePercentLabel.Text = "0%";
            NegativeCountLabel.Text = "(0 entries)";
            MissedDaysLabel.Text = "Missed Days: 0";
            return;
        }

        // Average words per entry
        int totalWords = ordered.Sum(e => CountWords(e.Content));
        double average = totalWords / (double)ordered.Count;
        AverageWordsLabel.Text = Math.Round(average).ToString("0");

        // Use distinct dates for streak calculations
        var dates = ordered
            .Select(e => e.EntryDate.Date)
            .Distinct()
            .OrderBy(d => d)
            .ToList();

        // Longest streak (max consecutive days anywhere in history)
        int longestStreak = 1;
        int currentRun = 1;
        for (int i = 1; i < dates.Count; i++)
        {
            if ((dates[i] - dates[i - 1]).TotalDays == 1)
            {
                currentRun++;
            }
            else
            {
                if (currentRun > longestStreak)
                    longestStreak = currentRun;
                currentRun = 1;
            }
        }
        if (currentRun > longestStreak)
            longestStreak = currentRun;

        // Current streak (ending today if you wrote today, or on last entry date)
        int currentStreak = 1;
        var lastDate = dates[^1];
        for (int i = dates.Count - 2; i >= 0; i--)
        {
            if ((lastDate - dates[i]).TotalDays == 1)
            {
                currentStreak++;
                lastDate = dates[i];
            }
            else
            {
                break;
            }
        }

        // Missed days between first and last entry date
        int missedDays = 0;
        var start = dates[0];
        var end = dates[^1];
        var dateSet = dates.ToHashSet();
        for (var d = start; d <= end; d = d.AddDays(1))
        {
            if (!dateSet.Contains(d))
                missedDays++;
        }

        CurrentStreakLabel.Text = $"{currentStreak} day(s)";
        LongestStreakLabel.Text = $"{longestStreak} day(s)";
        MissedDaysLabel.Text = $"Missed Days: {missedDays}";

        // Mood distribution based on primary mood
        int positive = 0, neutral = 0, negative = 0;
        foreach (var entry in ordered)
        {
            var mood = (entry.PrimaryMood ?? string.Empty).Trim();
            if (PositiveMoods.Contains(mood, StringComparer.OrdinalIgnoreCase))
                positive++;
            else if (NeutralMoods.Contains(mood, StringComparer.OrdinalIgnoreCase))
                neutral++;
            else if (NegativeMoods.Contains(mood, StringComparer.OrdinalIgnoreCase))
                negative++;
        }

        int moodTotal = positive + neutral + negative;
        if (moodTotal == 0)
            moodTotal = 1; // avoid divide by zero

        double positivePercent = positive * 100.0 / moodTotal;
        double neutralPercent = neutral * 100.0 / moodTotal;
        double negativePercent = negative * 100.0 / moodTotal;

        PositivePercentLabel.Text = $"{positivePercent:F0}%";
        PositiveCountLabel.Text = $"({positive})";
        PositiveProgressBar.Progress = positivePercent / 100.0;

        NeutralPercentLabel.Text = $"{neutralPercent:F0}%";
        NeutralCountLabel.Text = $"({neutral})";
        NeutralProgressBar.Progress = neutralPercent / 100.0;

        NegativePercentLabel.Text = $"{negativePercent:F0}%";
        NegativeCountLabel.Text = $"({negative})";
        NegativeProgressBar.Progress = negativePercent / 100.0;
    }

    public static int CountWords(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return 0;

        return text
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Length;
    }
}
