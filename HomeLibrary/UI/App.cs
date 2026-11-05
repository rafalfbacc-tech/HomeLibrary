using HomeLibrary.Models;
using HomeLibrary.Services;
using static HomeLibrary.UI.ConsoleInput;

namespace HomeLibrary.UI;

public class App(Library library, LibraryStorage storage)
{
    private readonly IExporter[] exporters = [new TxtExporter(), new JsonExporter()];

    public void Run()
    {
        while (true)
        {
            Clear();
            PrintMenu();
            var choice = ReadLine("> ");
            if (choice == "0") return;

            Clear();
            switch (choice)
            {
                case "1": LibraryRenderer.Render(library); break;
                case "2": ListBooks(); break;
                case "3": FindAndShow(); break;
                case "4": AddBook(); break;
                case "5": EditBook(); break;
                case "6": MoveBook(); break;
                case "7": RemoveBook(); break;
                case "8": AddShelf(); break;
                case "9": RemoveShelf(); break;
                case "e" or "E": Export(); break;
                default: Error("Unknown option."); break;
            }

            Pause();
        }
    }

    private void PrintMenu()
    {
        Console.WriteLine("  === HOME LIBRARY ===");
        Console.WriteLine($"  {library.Shelves.Count} shelves, {library.BookCount} books");
        Console.WriteLine();
        Console.WriteLine("  1 show library      6 move book");
        Console.WriteLine("  2 list books        7 remove book");
        Console.WriteLine("  3 find book         8 add shelf");
        Console.WriteLine("  4 add book          9 remove shelf");
        Console.WriteLine("  5 edit book         E export");
        Console.WriteLine("  0 exit");
        Console.WriteLine();
    }

    private void ListBooks()
    {
        var all = library.AllBooks().ToList();
        if (all.Count == 0) { Console.WriteLine("  The library is empty."); return; }
        PrintTable(all);
    }

    private void FindAndShow()
    {
        var book = SelectBook();
        if (book is not null) LibraryRenderer.Render(library, book);
    }

    private void AddBook()
    {
        if (library.Shelves.Count == 0)
        {
            Error("The library has no shelves.");
            if (!Confirm("Add a shelf now?")) return;
            AddShelf();
            if (library.Shelves.Count == 0) return;
        }

        Console.WriteLine("  --- NEW BOOK ---");
        var book = new Book();
        if (!FillBookDetails(book, isNew: true)) { Console.WriteLine("  Cancelled."); return; }

        if (!PlaceInteractively(book)) { Console.WriteLine("  Cancelled - the book was not added."); return; }

        Save();
        Success("Book added.");
        LibraryRenderer.Render(library, book);
    }

    private void EditBook()
    {
        var book = SelectBook();
        if (book is null) return;

        Console.WriteLine("  --- EDIT BOOK (Enter = no change) ---");
        if (!FillBookDetails(book, isNew: false)) return;
        Save();
        Success($"Changes saved: {book}");
    }

    private void MoveBook()
    {
        var book = SelectBook();
        if (book is null) return;

        var source = library.FindShelfOf(book)!;
        int oldPosition = book.Position;
        Console.WriteLine($"  Currently: shelf {source.Number}, slot {oldPosition}.");

        source.Remove(book);
        if (!PlaceInteractively(book))
        {
            source.Place(book, oldPosition); // restore
            Console.WriteLine("  Cancelled - the book stayed in its place.");
            return;
        }

        Save();
        Success("Book moved.");
        LibraryRenderer.Render(library, book);
    }

    private void RemoveBook()
    {
        var book = SelectBook();
        if (book is null) return;

        if (!Confirm($"Really remove {book}?")) return;
        library.FindShelfOf(book)!.Remove(book);
        Save();
        Success("Book removed.");
    }

    private static bool FillBookDetails(Book book, bool isNew)
    {
        var title = ReadText("Title", isNew ? null : book.Title);
        if (title is null) return false;

        var author = ReadText("Author", isNew ? null : book.Author);
        if (author is null) return false;

        int maxYear = DateTime.Now.Year;
        var yearHint = isNew || book.Year is null ? "Enter = none" : $"Enter = {book.Year}";
        var year = ReadInt($"Publication year (1-{maxYear}, {yearHint}): ", 1, maxYear, isNew ? null : book.Year);

        Console.WriteLine("  Cover:");
        var covers = Enum.GetValues<CoverType>();
        for (int i = 0; i < covers.Length; i++)
            Console.WriteLine($"  {i + 1}. {covers[i].DisplayName()}");
        var defaultCover = isNew ? CoverType.Hardcover : book.Cover;
        var coverNo = ReadInt($"Choose (1-{covers.Length}, Enter = {defaultCover.DisplayName()}): ",
            1, covers.Length, Array.IndexOf(covers, defaultCover) + 1)!.Value;

        book.Title = title;
        book.Author = author;
        book.Year = year;
        book.Cover = covers[coverNo - 1];
        return true;
    }

    private bool PlaceInteractively(Book book)
    {
        while (true)
        {
            int count = library.Shelves.Count;
            var shelfNo = ReadInt($"Shelf number (1-{count}, Enter = cancel): ", 1, count);
            if (shelfNo is null) return false;

            var shelf = library.GetShelf(shelfNo.Value)!;
            if (shelf.IsFull)
            {
                Error($"Shelf {shelf.Number} is full ({shelf.Capacity}/{shelf.Capacity}). Choose another one.");
                continue;
            }

            Console.WriteLine($"  Free slots on shelf {shelf.Number}: {FreeRanges(shelf)}");
            int firstFree = shelf.FirstFreePosition()!.Value;
            int pos = ReadInt($"Slot on shelf (1-{shelf.Capacity}, Enter = first free: {firstFree}): ",
                1, shelf.Capacity, firstFree)!.Value;

            if (shelf.IsFree(pos))
            {
                shelf.Place(book, pos);
                return true;
            }

            Console.WriteLine($"  Slot {pos} is taken by: {shelf.BookAt(pos)}");
            if (shelf.CanInsertWithShift(pos))
            {
                if (Confirm("Insert the book here and shift the following ones one slot to the right?"))
                {
                    shelf.InsertWithShift(book, pos);
                    return true;
                }
            }
            else
            {
                Error("There is no gap after this slot, so books cannot be shifted.");
            }
            Console.WriteLine("  Choose another slot.");
        }
    }

    private Book? SelectBook()
    {
        if (library.BookCount == 0) { Error("The library is empty."); return null; }

        var query = ReadLine("Search (part of title, author or year; Enter = all): ");
        var results = library.Search(query);
        if (results.Count == 0) { Error("No books found."); return null; }

        PrintTable(results);
        var no = ReadInt($"Choose a number from the list (1-{results.Count}, Enter = cancel): ", 1, results.Count);
        return no is null ? null : results[no.Value - 1].Book;
    }

    private void AddShelf()
    {
        var capacity = ReadInt($"How many slots should the new shelf have? (1-{Shelf.MaxCapacity}, Enter = {Shelf.MaxCapacity}): ",
            1, Shelf.MaxCapacity, Shelf.MaxCapacity)!.Value;
        var shelf = library.AddShelf(capacity);
        Save();
        Success($"Added shelf no. {shelf.Number} ({shelf.Capacity} slots).");
    }

    private void RemoveShelf()
    {
        if (library.Shelves.Count == 0) { Error("No shelves."); return; }

        foreach (var s in library.Shelves)
            Console.WriteLine($"  Shelf {s.Number}: {s.Count}/{s.Capacity} books");

        var no = ReadInt($"Which shelf to remove? (1-{library.Shelves.Count}, Enter = cancel): ", 1, library.Shelves.Count);
        if (no is null) return;

        var shelf = library.GetShelf(no.Value)!;
        var warning = shelf.Count > 0 ? $" {shelf.Count} book(s) will be removed with it!" : "";
        if (!Confirm($"Remove shelf {shelf.Number}?{warning}")) return;

        library.RemoveShelf(shelf);
        Save();
        Success("Shelf removed. The remaining shelves were renumbered.");
    }

    private void Export()
    {
        Console.WriteLine("  Format:");
        for (int i = 0; i < exporters.Length; i++)
            Console.WriteLine($"  {i + 1}. {exporters[i].FormatName}");
        var no = ReadInt($"Choose (1-{exporters.Length}, Enter = cancel): ", 1, exporters.Length);
        if (no is null) return;

        var exporter = exporters[no.Value - 1];
        var dataFile = storage.Path;
        var defaultPath = System.IO.Path.Combine(
            System.IO.Path.GetDirectoryName(dataFile) ?? "",
            System.IO.Path.GetFileNameWithoutExtension(dataFile) + "-export" + exporter.FileExtension);

        var path = ReadText("File path", defaultPath)!;
        if (!System.IO.Path.HasExtension(path)) path += exporter.FileExtension;

        if (System.IO.Path.GetFullPath(path) == System.IO.Path.GetFullPath(dataFile))
        {
            Error("This is the library data file - choose a different path.");
            return;
        }
        if (File.Exists(path) && !Confirm($"File {path} already exists. Overwrite?")) return;

        try
        {
            exporter.Export(library, path);
            Success($"Exported {library.BookCount} book(s) to {path}");
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            Error($"Could not export to {path}: {ex.Message}");
        }
    }

    private static void PrintTable(IReadOnlyList<(Shelf Shelf, Book Book)> rows)
    {
        const string format = "  {0,3}  {1,-5}  {2,-28}  {3,-18}  {4,4}  {5}";
        Console.WriteLine(format, "NO", "PLACE", "TITLE", "AUTHOR", "YEAR", "COVER");
        Console.WriteLine("  " + new string('-', 75));
        for (int i = 0; i < rows.Count; i++)
        {
            var (shelf, book) = rows[i];
            Console.WriteLine(format, i + 1, $"{shelf.Number}/{book.Position}", Cut(book.Title, 28), Cut(book.Author, 18),
                book.Year?.ToString() ?? "-", book.Cover.DisplayName());
        }
        Console.WriteLine("  " + new string('-', 75));
        Console.WriteLine($"  {rows.Count} book(s)   PLACE = shelf/slot");
    }

    private static string Cut(string s, int max) => s.Length <= max ? s : s[..max];

    private static string FreeRanges(Shelf shelf)
    {
        var slots = shelf.GetSlots();
        var parts = new List<string>();
        int p = 1;
        while (p <= shelf.Capacity)
        {
            if (slots[p] is not null) { p++; continue; }
            int from = p;
            while (p + 1 <= shelf.Capacity && slots[p + 1] is null) p++;
            parts.Add(from == p ? $"{from}" : $"{from}-{p}");
            p++;
        }
        return parts.Count == 0 ? "none" : string.Join(", ", parts);
    }

    private void Save()
    {
        try { storage.Save(library); }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            Error($"Could not save file {storage.Path}: {ex.Message}");
        }
    }
}
