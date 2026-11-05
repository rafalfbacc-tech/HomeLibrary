using HomeLibrary.Models;

namespace HomeLibrary.UI;

public static class LibraryRenderer
{
    private const int SlotsPerLine = 50;

    public static void Render(Library library, Book? highlight = null)
    {
        Console.WriteLine();
        Console.WriteLine("  === HOME LIBRARY ===");

        if (library.Shelves.Count == 0)
        {
            Console.WriteLine("  (no shelves - add the first shelf from the menu)");
            return;
        }

        foreach (var shelf in library.Shelves)
            RenderShelf(shelf, highlight);

        Console.WriteLine();
        Console.WriteLine("  # book   . empty   @ selected");

        if (highlight is not null && library.FindShelfOf(highlight) is { } found)
        {
            Console.WriteLine();
            ConsoleInput.WriteColored($"  > {highlight}\n    shelf {found.Number}, slot {highlight.Position}", ConsoleColor.Yellow);
        }
    }

    private static void RenderShelf(Shelf shelf, Book? highlight)
    {
        var slots = shelf.GetSlots();
        int width = Math.Min(shelf.Capacity, SlotsPerLine);
        var border = $"  +{new string('-', width)}+";

        Console.WriteLine();
        Console.Write($"  SHELF {shelf.Number}  {shelf.Count}/{shelf.Capacity}");
        if (highlight is not null && shelf.Books.Contains(highlight))
            ConsoleInput.WriteColored("  <-- HERE", ConsoleColor.Yellow, newLine: false);
        Console.WriteLine();
        Console.WriteLine(border);

        for (int start = 1; start <= shelf.Capacity; start += SlotsPerLine)
        {
            int end = Math.Min(shelf.Capacity, start + SlotsPerLine - 1);
            Console.Write("  |");
            for (int pos = start; pos <= end; pos++)
            {
                var book = slots[pos];
                if (book is not null && ReferenceEquals(book, highlight))
                    ConsoleInput.WriteColored("@", ConsoleColor.Yellow, newLine: false);
                else
                    Console.Write(book is null ? '.' : '#');
            }
            Console.WriteLine($"{new string(' ', width - (end - start + 1))}|  {start}-{end}");
        }

        Console.WriteLine(border);
    }
}
