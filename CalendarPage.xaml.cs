using JournalApp.Services;
using JournalApp.Models;

namespace JournalApp;

public partial class CalendarPage : ContentPage
{
    private readonly JournalService _journalService = new();

    public CalendarPage()
    {
        InitializeComponent();

        CalendarDatePicker.DateSelected += OnDateSelected;
    }

    private async void OnDateSelected(object? sender, DateChangedEventArgs e)
    {
        var selected = (e.NewDate ?? DateTime.Today).Date;
        var entry = await _journalService.GetEntryByDateAsync(selected);

        if (entry == null)
        {
            SelectedDateLabel.Text = $"No entry for {selected:yyyy-MM-dd}";
        }
        else
        {
            SelectedDateLabel.Text = $"{selected:yyyy-MM-dd}: {entry.Title} ({entry.PrimaryMood})";
        }
    }
}
