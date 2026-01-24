using System.Collections.ObjectModel;
using JournalApp.Models;
using JournalApp.Services;

namespace JournalApp;

public partial class EntriesPage : ContentPage
{
    private readonly JournalService _journalService = new();
    private readonly ObservableCollection<JournalEntry> _entries = new();
    private readonly List<JournalEntry> _allEntries = new();

    public EntriesPage()
    {
        InitializeComponent();

        EntriesCollectionView.ItemsSource = _entries;

        // Simple mood filter list (optional)
        MoodFilterPicker.ItemsSource = new[]
        {
            "Happy","Excited","Relaxed","Grateful","Confident",
            "Calm","Thoughtful","Curious","Nostalgic","Bored",
            "Sad","Angry","Stressed","Lonely","Anxious"
        };

        SearchEntry.TextChanged += OnFilterChanged;
        MoodFilterPicker.SelectedIndexChanged += OnFilterChanged;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadEntriesAsync();
    }

    private async Task LoadEntriesAsync()
    {
        _entries.Clear();
        _allEntries.Clear();

        var entries = await _journalService.GetEntriesAsync();
        _allEntries.AddRange(entries);

        ApplyFilter();
    }

    private void OnFilterChanged(object? sender, EventArgs e)
    {
        ApplyFilter();
    }

    private void ApplyFilter()
    {
        if (_allEntries.Count == 0)
        {
            EmptyLabel.IsVisible = true;
            EmptyLabel.Text = "No entries yet. Click 'New Entry' to create your first journal entry!";
            return;
        }

        var search = (SearchEntry.Text ?? string.Empty).Trim().ToLowerInvariant();
        var mood = MoodFilterPicker.SelectedItem as string;

        var filtered = _allEntries.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            filtered = filtered.Where(e =>
                (e.Title ?? string.Empty).ToLowerInvariant().Contains(search) ||
                (e.Content ?? string.Empty).ToLowerInvariant().Contains(search) ||
                (e.Tags ?? string.Empty).ToLowerInvariant().Contains(search));
        }

        if (!string.IsNullOrWhiteSpace(mood))
        {
            filtered = filtered.Where(e =>
                string.Equals(e.PrimaryMood, mood, StringComparison.OrdinalIgnoreCase) ||
                (e.SecondaryMoods ?? string.Empty).Contains(mood, StringComparison.OrdinalIgnoreCase));
        }

        var filteredList = filtered.ToList();
        
        _entries.Clear();
        foreach (var entry in filteredList)
            _entries.Add(entry);

        // Update empty state
        if (filteredList.Count == 0)
        {
            EmptyLabel.IsVisible = true;
            EmptyLabel.Text = "No entries match your search or filter criteria.";
        }
        else
        {
            EmptyLabel.IsVisible = false;
        }
    }

    private async void OnEntrySelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is JournalEntry entry)
        {
            ((CollectionView)sender).SelectedItem = null;
            await Navigation.PushAsync(new EntryDetailPage(entry));
        }
    }

    private void OnClearFiltersClicked(object sender, EventArgs e)
    {
        SearchEntry.Text = string.Empty;
        MoodFilterPicker.SelectedIndex = -1;
        ApplyFilter();
    }
}
