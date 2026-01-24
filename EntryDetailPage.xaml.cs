using System.Diagnostics;
using JournalApp.Models;
using JournalApp.Services;

namespace JournalApp;

public partial class EntryDetailPage : ContentPage
{
    private readonly JournalService _journalService = new();
    private JournalEntry _entry;

    public EntryDetailPage(JournalEntry entry)
    {
        InitializeComponent();
        _entry = entry;
        LoadMoods();
        LoadEntryData();
    }

    private void LoadMoods()
    {
        MoodPicker.ItemsSource = new[]
        {
            "Happy", "Sad", "Anxious", "Excited", "Calm", "Angry",
            "Neutral", "Grateful", "Confident", "Stressed"
        };
    }

    private void LoadEntryData()
    {
        TitleEntry.Text = _entry.Title;
        EntryDatePicker.Date = _entry.EntryDate;
        MoodPicker.SelectedItem = _entry.PrimaryMood;
        TagsEntry.Text = _entry.Tags;
        ContentEditor.Text = _entry.Content;
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(TitleEntry.Text))
            {
                await DisplayAlert("Validation Error", "Please enter a title", "OK");
                return;
            }

            if (MoodPicker.SelectedItem == null)
            {
                await DisplayAlert("Validation Error", "Please select a mood", "OK");
                return;
            }

            _entry.Title = TitleEntry.Text;
            _entry.EntryDate = EntryDatePicker.Date ?? DateTime.Now;
            _entry.PrimaryMood = MoodPicker.SelectedItem.ToString() ?? "";
            _entry.Tags = TagsEntry.Text ?? "";
            _entry.Content = ContentEditor.Text ?? "";
            _entry.UpdateWordCount();

            await _journalService.UpdateEntryAsync(_entry);
            
            await DisplayAlert("Success", "Entry updated successfully!", "OK");
            await Navigation.PopAsync();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error saving entry: {ex.Message}");
            await DisplayAlert("Error", $"Failed to save entry: {ex.Message}", "OK");
        }
    }

    private async void OnDeleteClicked(object sender, EventArgs e)
    {
        try
        {
            bool confirm = await DisplayAlert(
                "Confirm Delete", 
                "Are you sure you want to delete this entry?", 
                "Delete", 
                "Cancel");

            if (!confirm)
                return;

            await _journalService.DeleteEntryAsync(_entry.Id);
            
            await DisplayAlert("Success", "Entry deleted successfully!", "OK");
            await Navigation.PopAsync();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error deleting entry: {ex.Message}");
            await DisplayAlert("Error", $"Failed to delete entry: {ex.Message}", "OK");
        }
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}
