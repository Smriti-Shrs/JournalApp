using JournalApp.Models;
using JournalApp.Services;

namespace JournalApp;

public partial class AnalyticsPage : ContentPage
{
    private readonly JournalService _journalService = new();

    public AnalyticsPage()
    {
        InitializeComponent();
        FromDatePicker.Date = DateTime.Today.AddMonths(-1);
        ToDatePicker.Date = DateTime.Today;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await UpdateAnalyticsAsync();
    }

    private async void OnUpdateAnalyticsClicked(object sender, EventArgs e)
    {
        await UpdateAnalyticsAsync();
    }

    private async Task UpdateAnalyticsAsync()
    {
        var from = (FromDatePicker.Date ?? DateTime.Today.AddMonths(-1)).Date;
        var to = (ToDatePicker.Date ?? DateTime.Today).Date;

        var allEntries = await _journalService.GetEntriesAsync();
        var entries = allEntries
            .Where(e => e.EntryDate.Date >= from && e.EntryDate.Date <= to)
            .OrderBy(e => e.EntryDate.Date)
            .ToList();

        TotalEntriesRangeLabel.Text = entries.Count.ToString();

        if (entries.Count == 0)
        {
            CurrentStreakRangeLabel.Text = "0";
            LongestStreakRangeLabel.Text = "0";
            MissedDaysRangeLabel.Text = "0";
            MoodDistributionLabel.Text = "No entries in this range.";
            MostFrequentMoodLabel.Text = "-";
            TotalWordsLabel.Text = "0";
            AveragePerEntryLabel.Text = "0";
            return;
        }

        // Distinct dates for streaks
        var dates = entries.Select(e => e.EntryDate.Date).Distinct().OrderBy(d => d).ToList();

        // Longest streak
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

        // Current streak in range (ending on last date)
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

        CurrentStreakRangeLabel.Text = currentStreak.ToString();
        LongestStreakRangeLabel.Text = longestStreak.ToString();

        // Missed days in range
        int missed = 0;
        var dateSet = dates.ToHashSet();
        for (var d = from; d <= to; d = d.AddDays(1))
        {
            if (!dateSet.Contains(d))
                missed++;
        }
        MissedDaysRangeLabel.Text = missed.ToString();

        // Mood distribution and most frequent mood
        var moodGroups = entries
            .GroupBy(e => e.PrimaryMood ?? "Unknown")
            .OrderByDescending(g => g.Count())
            .ToList();

        int totalMoods = moodGroups.Sum(g => g.Count());
        var sb = new System.Text.StringBuilder();
        foreach (var g in moodGroups)
        {
            int pct = g.Count() * 100 / totalMoods;
            sb.AppendLine($"{g.Key}: {pct}% ({g.Count()} entries)");
        }
        MoodDistributionLabel.Text = sb.ToString().TrimEnd();

        var most = moodGroups.First();
        MostFrequentMoodLabel.Text = $"{most.Key} appeared {most.Count()} time(s).";

        // Writing stats
        int totalWords = entries.Sum(e => DashboardPage.CountWords(e.Content));
        double avg = totalWords / (double)entries.Count;
        TotalWordsLabel.Text = totalWords.ToString();
        AveragePerEntryLabel.Text = Math.Round(avg).ToString("0");
    }
}
