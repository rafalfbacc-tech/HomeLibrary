using System.Text.Json;
using System.Text.Json.Serialization;
using HomeLibrary.Models;

namespace HomeLibrary.Services;

public class LibraryStorage(string path)
{
    internal static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public string Path { get; } = path;
    public bool Exists => File.Exists(Path);

    public Library Load()
    {
        if (!Exists) return new Library();

        var library = JsonSerializer.Deserialize<Library>(File.ReadAllText(Path), JsonOptions) ?? new Library();
        library.Normalize();
        return library;
    }

    public void Save(Library library)
    {
        var tmp = Path + ".tmp";
        File.WriteAllText(tmp, JsonSerializer.Serialize(library, JsonOptions));
        File.Move(tmp, Path, overwrite: true);
    }
}
