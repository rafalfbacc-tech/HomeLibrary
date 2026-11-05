using System.Text;
using HomeLibrary.Models;

namespace HomeLibrary.Services;

public class TxtExporter : IExporter
{
    public string FormatName => "TXT";
    public string FileExtension => ".txt";

    private const string Format = "{0,4}  {1,-30}  {2,-20}  {3,4}  {4}";
    private static readonly string Line = new('-', 75);

    public void Export(Library library, string path)
    {
        var sb = new StringBuilder();
        sb.AppendLine("=== HOME LIBRARY ===");
        sb.AppendLine($"{library.Shelves.Count} shelves, {library.BookCount} books, exported {DateTime.Now:yyyy-MM-dd HH:mm}");

        foreach (var shelf in library.Shelves)
        {
            sb.AppendLine();
            sb.AppendLine($"SHELF {shelf.Number}  {shelf.Count}/{shelf.Capacity}");
            sb.AppendLine(Line);
            if (shelf.Count == 0)
                sb.AppendLine("(empty)");
            else
            {
                sb.AppendFormat(Format, "SLOT", "TITLE", "AUTHOR", "YEAR", "COVER").AppendLine();
                foreach (var book in shelf.Books)
                    sb.AppendFormat(Format, book.Position, book.Title, book.Author,
                        book.Year?.ToString() ?? "-", book.Cover.DisplayName()).AppendLine();
            }
            sb.AppendLine(Line);
        }

        File.WriteAllText(path, sb.ToString(), Encoding.UTF8);
    }
}
