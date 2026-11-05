namespace HomeLibrary.Models;

public class Library
{
    public List<Shelf> Shelves { get; set; } = new();

    public int BookCount => Shelves.Sum(s => s.Count);

    public Shelf AddShelf(int capacity = Shelf.MaxCapacity)
    {
        if (capacity < 1 || capacity > Shelf.MaxCapacity)
            throw new ArgumentOutOfRangeException(nameof(capacity), $"Shelf capacity must be between 1 and {Shelf.MaxCapacity}.");

        var shelf = new Shelf { Number = Shelves.Count + 1, Capacity = capacity };
        Shelves.Add(shelf);
        return shelf;
    }

    public void RemoveShelf(Shelf shelf)
    {
        Shelves.Remove(shelf);
        Normalize();
    }

    public Shelf? GetShelf(int number) => Shelves.FirstOrDefault(s => s.Number == number);

    public Shelf? FindShelfOf(Book book) => Shelves.FirstOrDefault(s => s.Books.Contains(book));

    public IEnumerable<(Shelf Shelf, Book Book)> AllBooks() =>
        Shelves.SelectMany(s => s.Books.Select(b => (s, b)));

    public List<(Shelf Shelf, Book Book)> Search(string query)
    {
        query = query.Trim();
        if (query.Length == 0) return AllBooks().ToList();

        return AllBooks().Where(x =>
                x.Book.Title.Contains(query, StringComparison.CurrentCultureIgnoreCase) ||
                x.Book.Author.Contains(query, StringComparison.CurrentCultureIgnoreCase) ||
                x.Book.Year?.ToString() == query)
            .ToList();
    }

    public void Normalize()
    {
        for (int i = 0; i < Shelves.Count; i++)
        {
            Shelves[i].Number = i + 1;
            Shelves[i].Sort();
        }
    }
}
