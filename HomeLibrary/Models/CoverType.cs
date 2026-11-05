namespace HomeLibrary.Models;

public enum CoverType
{
    Hardcover,
    Paperback,
    Integrated,
    Spiral
}

public static class CoverTypeExtensions
{
    public static string DisplayName(this CoverType cover) => cover.ToString().ToLowerInvariant();
}
