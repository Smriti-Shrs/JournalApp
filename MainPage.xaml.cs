using JournalApp.Models;
using JournalApp.Services;
using System.Collections.ObjectModel;

namespace JournalApp;

public partial class MainPage : ContentPage
{
    private readonly JournalService _journalService = new();
    private readonly ObservableCollection<JournalEntry> _entries = new();

    private readonly string[] _allMoods = new[]
    {
        // Positive
        "Happy", "Excited", "Relaxed", "Grateful", "Confident",
        // Neutral
        "Calm", "Thoughtful", "Curious", "Nostalgic", "Bored",
        // Negative
        "Sad", "Angry", "Stressed", "Lonely", "Anxious"
    };

    public MainPage()
    {
        InitializeComponent();

        EntryDatePicker.Date = DateTime.Today;
        ThemeService.SetTheme(false); // start with light theme by default

        // Populate mood pickers
        PrimaryMoodPicker.ItemsSource = _allMoods;
        SecondaryMood1Picker.ItemsSource = _allMoods;
        SecondaryMood2Picker.ItemsSource = _allMoods;

        EntriesCollectionView.ItemsSource = _entries;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadEntriesAsync();

        var selectedDate = EntryDatePicker.Date ?? DateTime.Today;
        await LoadEntryForDateAsync(selectedDate);
    }

    private async Task LoadEntriesAsync()
    {
        _entries.Clear();
        var entries = await _journalService.GetEntriesAsync();
        foreach (var entry in entries)
        {
            _entries.Add(entry);
        }
    }

    private async Task LoadEntryForDateAsync(DateTime date)
    {
        var existing = await _journalService.GetEntryByDateAsync(date);
        if (existing == null)
        {
            ClearEditorFields();
            return;
        }

        TitleEntry.Text = existing.Title;
        EntryEditor.Text = existing.Content;
        PrimaryMoodPicker.SelectedItem = string.IsNullOrWhiteSpace(existing.PrimaryMood)
            ? null
            : existing.PrimaryMood;

        // Secondary moods stored as comma-separated
        var secondary = (existing.SecondaryMoods ?? string.Empty)
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        SecondaryMood1Picker.SelectedItem = secondary.Length > 0 ? secondary[0] : null;
        SecondaryMood2Picker.SelectedItem = secondary.Length > 1 ? secondary[1] : null;

        TagsEntry.Text = existing.Tags;
    }

    private void ClearEditorFields()
    {
        TitleEntry.Text = string.Empty;
        EntryEditor.Text = string.Empty;
        PrimaryMoodPicker.SelectedItem = null;
        SecondaryMood1Picker.SelectedItem = null;
        SecondaryMood2Picker.SelectedItem = null;
        TagsEntry.Text = string.Empty;
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        var date = (EntryDatePicker.Date ?? DateTime.Today).Date;

        if (PrimaryMoodPicker.SelectedItem is not string primaryMood || string.IsNullOrWhiteSpace(primaryMood))
        {
            await DisplayAlertAsync("Validation", "Primary mood is required.", "OK");
            return;
        }

        var title = string.IsNullOrWhiteSpace(TitleEntry.Text)
            ? $"Entry for {date:yyyy-MM-dd}"
            : TitleEntry.Text.Trim();

        var content = EntryEditor.Text ?? string.Empty;

        // Build secondary moods (up to two)
        var secondaryMoods = new List<string>();
        var s1 = SecondaryMood1Picker.SelectedItem as string;
        if (!string.IsNullOrWhiteSpace(s1))
            secondaryMoods.Add(s1);

        var s2 = SecondaryMood2Picker.SelectedItem as string;
        if (!string.IsNullOrWhiteSpace(s2) && s2 != s1)
            secondaryMoods.Add(s2);

        var tags = (TagsEntry.Text ?? string.Empty).Trim();
        var now = DateTime.Now;

        var existing = await _journalService.GetEntryByDateAsync(date);
        if (existing == null)
        {
            var newEntry = new JournalEntry
            {
                Title = title,
                Content = content,
                EntryDate = date,
                CreatedAt = now,
                UpdatedAt = now,
                PrimaryMood = primaryMood,
                SecondaryMoods = string.Join(", ", secondaryMoods),
                Tags = tags
            };

            await _journalService.AddEntryAsync(newEntry);
        }
        else
        {
            existing.Title = title;
            existing.Content = content;
            existing.EntryDate = date;
            existing.PrimaryMood = primaryMood;
            existing.SecondaryMoods = string.Join(", ", secondaryMoods);
            existing.Tags = tags;
            existing.UpdatedAt = now;

            await _journalService.UpdateEntryAsync(existing);
        }

        await LoadEntriesAsync();
        await LoadEntryForDateAsync(date);

        await DisplayAlertAsync("Saved", "Journal entry saved for this date.", "OK");
    }

    private async void OnDeleteClicked(object sender, EventArgs e)
    {
        var date = (EntryDatePicker.Date ?? DateTime.Today).Date;
        var existing = await _journalService.GetEntryByDateAsync(date);
        if (existing == null)
        {
            await DisplayAlertAsync("Delete", "No entry exists for this date.", "OK");
            return;
        }

        var confirm = await DisplayAlertAsync("Delete", "Delete the entry for this date?", "Yes", "No");
        if (!confirm)
            return;

        await _journalService.DeleteEntryAsync(existing.Id);
        await LoadEntriesAsync();
        ClearEditorFields();

        await DisplayAlertAsync("Deleted", "Entry deleted.", "OK");
    }

    private async void OnEntrySelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is not JournalEntry selected)
            return;

        // When an entry is selected, jump the date picker and load its data
        EntryDatePicker.Date = selected.EntryDate.Date;
        await LoadEntryForDateAsync(selected.EntryDate);
    }

    private void OnToggleThemeClicked(object sender, EventArgs e)
    {
        ThemeService.ToggleTheme();
    }
}
