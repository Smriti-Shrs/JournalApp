using JournalApp.Services;
using JournalApp.Models;

namespace JournalApp;

public partial class SettingsPage : ContentPage
{
    private readonly JournalService _journalService = new();

    public SettingsPage()
    {
        InitializeComponent();

        DarkThemeSwitch.IsToggled = ThemeService.IsDark;
        ExportFromDatePicker.Date = DateTime.Today.AddMonths(-1);
        ExportToDatePicker.Date = DateTime.Today;
    }

    private void OnDarkThemeToggled(object sender, ToggledEventArgs e)
    {
        ThemeService.SetTheme(e.Value);
    }

    private async void OnExportPdfClicked(object sender, EventArgs e)
    {
        var from = (ExportFromDatePicker.Date ?? DateTime.Today.AddMonths(-1)).Date;
        var to = (ExportToDatePicker.Date ?? DateTime.Today).Date;

        if (from > to)
        {
            await DisplayAlertAsync("Export", "From date must be before To date.", "OK");
            return;
        }

        var allEntries = await _journalService.GetEntriesAsync();
        var entries = allEntries
            .Where(e => e.EntryDate.Date >= from && e.EntryDate.Date <= to)
            .OrderBy(e => e.EntryDate.Date)
            .ToList();

        if (entries.Count == 0)
        {
            await DisplayAlertAsync("Export", "No entries in the selected date range.", "OK");
            return;
        }

        try
        {
            var path = PdfExportService.ExportEntries(entries, from, to);
            await DisplayAlertAsync("Export", $"PDF saved to:\n{path}", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Export Error", ex.Message, "OK");
        }
    }
}
