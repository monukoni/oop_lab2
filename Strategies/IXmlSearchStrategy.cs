using XmlLibraryLab2.Models;

namespace XmlLibraryLab2.Strategies;

public interface IXmlSearchStrategy
{
    string Name { get; }
    Task<IReadOnlyList<string>> SearchAsync(string xmlPath, SearchQuery query, CancellationToken ct);
}