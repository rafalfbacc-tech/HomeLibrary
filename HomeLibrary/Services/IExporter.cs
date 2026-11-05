using HomeLibrary.Models;

namespace HomeLibrary.Services;

public interface IExporter
{
    string FormatName { get; }
    string FileExtension { get; }

    void Export(Library library, string path);
}
