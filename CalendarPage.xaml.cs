using System.Collections.ObjectModel;
using JournalApp.Services;
using JournalApp.Models;

namespace JournalApp;

public class CalendarDayViewModel
{
    public DateTime Date { get; set; }
    public int DayNumber => Date.Day;
    public string MoodEmoji { get; set; } = string.Empty;
    public Color BackgroundColor { get; set; } = Colors.Transparent;
    public bool HasEntry { get; set; }
}

public partial class CalendarPage : ContentPage
{
    private readonly JournalService _journalService = new();
    private readonly ObservableCollection<CalendarDayViewModel> _days = new();

    private DateTime _currentMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);

    public CalendarPage()
    {
        InitializeComponent();

        CalendarCollectionView.ItemsSource = _days;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        BuildCalendar();
    }

    private void OnPreviousMonthClicked(object sender, EventArgs e)
    {
        _currentMonth = _currentMonth.AddMonths(-1);
        BuildCalendar();
    }

    private void OnNextMonthClicked(object sender, EventArgs e)
    {
        _currentMonth = _currentMonth.AddMonths(1);
        BuildCalendar();
    }

    private async void BuildCalendar()
    {
        MonthLabel.Text = _currentMonth.ToString("MMMM yyyy");

        _days.Clear();

        // Load all entries once and index by date
        var entries = await _journalService.GetEntriesAsync();
        var byDate = entries
            .GroupBy(e => e.EntryDate.Date)
            .ToDictionary(g => g.Key, g => g.First());

        // Determine first cell date (start from Sunday of first week including 1st of month)
        var firstOfMonth = _currentMonth;
        int offset = (int)firstOfMonth.DayOfWeek;
        var startDate = firstOfMonth.AddDays(-offset);

        for (int i = 0; i < 42; i++)
        {
            var date = startDate.AddDays(i);
            var dayVm = new CalendarDayViewModel { Date = date };

            // Default background for days in current month / other months
            bool inCurrentMonth = date.Month == _currentMonth.Month && date.Year == _currentMonth.Year;
            dayVm.BackgroundColor = inCurrentMonth ? Colors.White : Color.FromArgb("#ECEFF1");

            if (byDate.TryGetValue(date.Date, out var entry))
            {
                dayVm.HasEntry = true;
                // Color based on mood category
                var mood = (entry.PrimaryMood ?? string.Empty).Trim();
                if (DashboardPage.PositiveMoods.Contains(mood, StringComparer.OrdinalIgnoreCase))
                {
                    dayVm.BackgroundColor = Color.FromArgb("#A5D6A7");
                    dayVm.MoodEmoji = "😊";
                }
                else if (DashboardPage.NeutralMoods.Contains(mood, StringComparer.OrdinalIgnoreCase))
                {
                    dayVm.BackgroundColor = Color.FromArgb("#90CAF9");
                    dayVm.MoodEmoji = "😐";
                }
                else if (DashboardPage.NegativeMoods.Contains(mood, StringComparer.OrdinalIgnoreCase))
                {
                    dayVm.BackgroundColor = Color.FromArgb("#EF9A9A");
                    dayVm.MoodEmoji = "☹";
                }
            }

            // Highlight today with yellow tint
            if (date.Date == DateTime.Today.Date)
            {
                dayVm.BackgroundColor = Color.FromArgb("#FFF59D");
            }

            _days.Add(dayVm);
        }
    }

    private async void OnDaySelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is not CalendarDayViewModel day)
            return;

        ((CollectionView)sender).SelectedItem = null;

        var entry = await _journalService.GetEntryByDateAsync(day.Date);
        
        if (entry != null)
        {
            // Entry exists - open detail page to view/edit
            await Navigation.PushAsync(new EntryDetailPage(entry));
        }
        else
        {
            // No entry - ask if they want to create one
            bool create = await DisplayAlert(
                $"No Entry for {day.Date:MMM dd, yyyy}",
                "Would you like to create an entry for this date?",
                "Create",
                "Cancel");

            if (create)
            {
                // Navigate to New Entry page (MainPage)
                await Shell.Current.GoToAsync("//New Entry");
            }
        }
    }
}
