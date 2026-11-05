namespace HomeLibrary.Models;

public class Book
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = "";
    public string Author { get; set; } = "";
    public int? Year { get; set; }
    public CoverType Cover { get; set; } = CoverType.Hardcover;
    public int Position { get; set; }

    public override string ToString() =>
        $"\"{Title}\" - {Author}{(Year is int y ? $" ({y})" : "")}, {Cover.DisplayName()}";
}
