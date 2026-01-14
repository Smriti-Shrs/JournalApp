using JournalApp.Models;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using QColors = QuestPDF.Helpers.Colors;

namespace JournalApp.Services;

public static class PdfExportService
{
    public static string ExportEntries(IEnumerable<JournalEntry> entries, DateTime from, DateTime to)
    {
        var ordered = entries.OrderBy(e => e.EntryDate).ToList();

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(30);
                page.Size(QuestPDF.Helpers.PageSizes.A4);

                page.Header()
                    .Text($"Journal Export {from:yyyy-MM-dd} - {to:yyyy-MM-dd}")
                    .FontSize(20)
                    .Bold();

                page.Content().Column(col =>
                {
                    foreach (var entry in ordered)
                    {
                        col.Item().PaddingBottom(10).BorderBottom(1).BorderColor(QColors.Grey.Lighten2).Element(section =>
                        {
                            section.Column(inner =>
                            {
                                inner.Item().Text($"{entry.EntryDate:yyyy-MM-dd} - {entry.Title} ({entry.PrimaryMood})").Bold();
                                if (!string.IsNullOrWhiteSpace(entry.Tags))
                                    inner.Item().Text($"Tags: {entry.Tags}").FontSize(10);

                                inner.Item().Text(entry.Content ?? string.Empty).FontSize(11);
                            });
                        });
                    }
                });

                page.Footer().AlignRight().Text(x =>
                {
                    x.Span("Page ");
                    x.CurrentPageNumber();
                    x.Span(" / ");
                    x.TotalPages();
                });
            });
        });

        var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        var fileName = $"JournalExport_{from:yyyyMMdd}_{to:yyyyMMdd}.pdf";
        var filePath = Path.Combine(documentsPath, fileName);

        document.GeneratePdf(filePath);
        return filePath;
    }
}
