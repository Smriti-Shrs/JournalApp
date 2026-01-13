using System.Collections.ObjectModel;
using JournalApp.Models;
using JournalApp.Services;

namespace JournalApp;

public partial class DashboardPage : ContentPage
{
    private readonly JournalService _journalService = new();
    private readonly ObservableCollection<JournalEntry> _recentEntries = new();

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
            MissedDaysLabel.Text = "0";
            return;
        }

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
        MissedDaysLabel.Text = missedDays.ToString();
    }
}
