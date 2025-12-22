using System.Xml.Linq;
using XmlLibraryLab2.Models;

namespace XmlLibraryLab2.Strategies;

public sealed class LinqXmlSearchStrategy : IXmlSearchStrategy
{
    public string Name => "LINQ to XML";

    public Task<IReadOnlyList<string>> SearchAsync(string xmlPath, SearchQuery query, CancellationToken ct)
    {
        var doc = XDocument.Load(xmlPath);
        var nodes = doc.Descendants(query.MainNodeName);

        if (!string.IsNullOrWhiteSpace(query.AttributeName) && query.AttributeValue != null)
            nodes = nodes.Where(n => (string?)n.Attribute(query.AttributeName) == query.AttributeValue);

        if (!string.IsNullOrWhiteSpace(query.Keyword))
        {
            var kw = query.Keyword.Trim();
            nodes = nodes.Where(n =>
                ((string?)n.Element("title"))?.Contains(kw, StringComparison.OrdinalIgnoreCase) == true ||
                ((string?)n.Element("annotation"))?.Contains(kw, StringComparison.OrdinalIgnoreCase) == true ||
                ((string?)n.Element("author")?.Attribute("fullName"))?.Contains(kw, StringComparison.OrdinalIgnoreCase) == true
            );
        }

        var res = nodes.Select(n =>
        {
            var id = (string?)n.Attribute("id") ?? "-";
            var year = (string?)n.Attribute("year") ?? "-";
            var author = (string?)n.Element("author")?.Attribute("fullName") ?? "(no author)";
            var title = (string?)n.Element("title") ?? "(no title)";
            return $"{id} | {year} | {author} — {title}";
        }).ToList();

        return Task.FromResult<IReadOnlyList<string>>(res);
    }
}