using HomeLibrary.Models;

namespace HomeLibrary.Services;

public static class SampleData
{
    public static Library Create()
    {
        var lib = new Library();
        var s1 = lib.AddShelf(30);
        var s2 = lib.AddShelf(40);
        var s3 = lib.AddShelf(25);

        s1.Place(B("Pan Tadeusz", "Adam Mickiewicz", 1834, CoverType.Hardcover), 1);
        s1.Place(B("Quo Vadis", "Henryk Sienkiewicz", 1896, CoverType.Hardcover), 2);

        s2.Place(B("Dune", "Frank Herbert", 1965, CoverType.Hardcover), 1);
        s2.Place(B("The Hobbit", "J.R.R. Tolkien", 1937, CoverType.Integrated),2);
        s2.Place(B("Nineteen Eighty-Four", "George Orwell", 1949, CoverType.Paperback), 3);

        s3.Place(B("Random", "various authors", 1955, CoverType.Spiral), 1);
        return lib;
    }

    private static Book B(string title, string author, int year, CoverType cover) =>
        new() { Title = title, Author = author, Year = year, Cover = cover };
}
