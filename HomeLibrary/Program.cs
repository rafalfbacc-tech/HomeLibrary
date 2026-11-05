using System.Text;
using System.Text.Json;
using HomeLibrary.Models;
using HomeLibrary.Services;
using HomeLibrary.UI;

Console.OutputEncoding = Encoding.UTF8;
if (Console.IsInputRedirected) Console.InputEncoding = Encoding.UTF8;

var path = args.Length > 0 ? args[0] : Path.Combine(Environment.CurrentDirectory, "library.json");
var storage = new LibraryStorage(path);

try
{
    Library library;
    try
    {
        library = storage.Load();
    }
    catch (JsonException ex)
    {
        ConsoleInput.Error($"File {path} is corrupted: {ex.Message}");
        var backup = path + ".corrupted";
        File.Copy(path, backup, overwrite: true);
        Console.WriteLine($"  Backup: {backup}. Starting with an empty library.");
        library = new Library();
    }

    if (!storage.Exists && library.Shelves.Count == 0 && ConsoleInput.Confirm("No saved library found. Load sample data?"))
    {
        library = SampleData.Create();
    }

    storage.Save(library);
    new App(library, storage).Run();
    storage.Save(library);
    Console.WriteLine($"Saved to {path}. Bye.");
}
catch (EndOfInputException)
{
    Console.WriteLine();
    Console.WriteLine("End of input - exiting.");
}
