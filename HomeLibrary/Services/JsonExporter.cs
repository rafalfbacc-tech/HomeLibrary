using System.Text.Json;
using HomeLibrary.Models;

namespace HomeLibrary.Services;

public class JsonExporter : IExporter
{
    public string FormatName => "JSON";
    public string FileExtension => ".json";

    public void Export(Library library, string path) =>
        File.WriteAllText(path, JsonSerializer.Serialize(library, LibraryStorage.JsonOptions));
}
