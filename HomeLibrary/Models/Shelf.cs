using System.Text.Json.Serialization;

namespace HomeLibrary.Models;

/// <summary>A shelf with numbered slots 1..Capacity (max 100).</summary>
public class Shelf
{
    public const int MaxCapacity = 100;

    public int Number { get; set; }
    public int Capacity { get; set; } = MaxCapacity;
    public List<Book> Books { get; set; } = new();

    [JsonIgnore] public int Count => Books.Count;
    [JsonIgnore] public bool IsFull => Books.Count >= Capacity;

    public Book? BookAt(int position) => Books.FirstOrDefault(b => b.Position == position);

    /// <summary>
    /// Snapshot of the shelf as an array indexed by slot number (index 0 is unused).
    /// Use it instead of calling <see cref="BookAt"/> for every slot in a loop.
    /// </summary>
    public Book?[] GetSlots()
    {
        var slots = new Book?[Capacity + 1];
        foreach (var book in Books)
            if (IsValidPosition(book.Position))
                slots[book.Position] ??= book;
        return slots;
    }

    public bool IsValidPosition(int position) => position >= 1 && position <= Capacity;

    public bool IsFree(int position) => IsValidPosition(position) && BookAt(position) is null;

    public int? FirstFreePosition() => FreeAtOrAfter(1);

    /// <summary>Places a book on a free slot.</summary>
    public void Place(Book book, int position)
    {
        if (!IsValidPosition(position))
            throw new ArgumentOutOfRangeException(nameof(position), $"Slot must be between 1 and {Capacity}.");
        if (BookAt(position) is { } other)
            throw new InvalidOperationException($"Slot {position} is occupied by \"{other.Title}\".");

        book.Position = position;
        Books.Add(book);
        Sort();
    }

    /// <summary>Whether a book can be inserted at <paramref name="position"/> by shifting the following ones to the right.</summary>
    public bool CanInsertWithShift(int position) => IsValidPosition(position) && FreeAtOrAfter(position) is not null;

    /// <summary>
    /// Inserts a book at the given slot. If it is occupied, books from that slot
    /// up to the nearest free gap are shifted one slot to the right.
    /// </summary>
    public void InsertWithShift(Book book, int position)
    {
        if (!IsValidPosition(position))
            throw new ArgumentOutOfRangeException(nameof(position), $"Slot must be between 1 and {Capacity}.");
        int gap = FreeAtOrAfter(position)
            ?? throw new InvalidOperationException("No free slot after the given position - cannot shift books.");

        foreach (var b in Books.Where(b => b.Position >= position && b.Position < gap))
            b.Position++;

        Place(book, position);
    }

    public bool Remove(Book book) => Books.Remove(book);

    public void Sort() => Books.Sort((a, b) => a.Position.CompareTo(b.Position));

    private int? FreeAtOrAfter(int position)
    {
        var slots = GetSlots();
        for (int p = Math.Max(position, 1); p <= Capacity; p++)
            if (slots[p] is null) return p;
        return null;
    }
}
