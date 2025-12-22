using System.Xml;
using XmlLibraryLab2.Models;

namespace XmlLibraryLab2.Strategies;

public sealed class DomXmlSearchStrategy : IXmlSearchStrategy
{
    public string Name => "DOM API";

    public Task<IReadOnlyList<string>> SearchAsync(string xmlPath, SearchQuery query, CancellationToken ct)
    {
        var doc = new XmlDocument();
        doc.Load(xmlPath);

        var list = doc.GetElementsByTagName(query.MainNodeName);
        var res = new List<string>();

        foreach (XmlNode node in list)
        {
            if (node is not XmlElement el) continue;

            if (!string.IsNullOrWhiteSpace(query.AttributeName) && query.AttributeValue != null)
            {
                var val = el.GetAttribute(query.AttributeName);
                if (!string.Equals(val, query.AttributeValue, StringComparison.Ordinal)) continue;
            }

            if (!string.IsNullOrWhiteSpace(query.Keyword))
            {
                var kw = query.Keyword.Trim();

                string title = el["title"]?.InnerText ?? "";
                string annotation = el["annotation"]?.InnerText ?? "";
                string author = el["author"]?.GetAttribute("fullName") ?? "";

                bool ok =
                    title.Contains(kw, StringComparison.OrdinalIgnoreCase) ||
                    annotation.Contains(kw, StringComparison.OrdinalIgnoreCase) ||
                    author.Contains(kw, StringComparison.OrdinalIgnoreCase);

                if (!ok) continue;
            }

            var id = el.GetAttribute("id");
            var year = el.GetAttribute("year");
            var authorName = el["author"]?.GetAttribute("fullName") ?? "(no author)";
            var titleText = el["title"]?.InnerText ?? "(no title)";

            res.Add($"{id} | {year} | {authorName} — {titleText}");
        }

        return Task.FromResult<IReadOnlyList<string>>(res);
    }
}