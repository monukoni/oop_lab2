using System.Xml;
using XmlLibraryLab2.Models;

namespace XmlLibraryLab2.Strategies;

public sealed class SaxXmlSearchStrategy : IXmlSearchStrategy
{
    public string Name => "SAX API (XmlReader)";

    public async Task<IReadOnlyList<string>> SearchAsync(string xmlPath, SearchQuery query, CancellationToken ct)
    {
        var results = new List<string>();

        using var fs = File.OpenRead(xmlPath);
        using var reader = XmlReader.Create(fs, new XmlReaderSettings
        {
            Async = true,
            IgnoreComments = true,
            IgnoreWhitespace = true
        });

        bool inTarget = false;
        bool rejectedByAttr = false;

        string? curId = null, curYear = null, curTitle = null, curAnnotation = null, curAuthor = null;

        void Reset()
        {
            inTarget = false;
            rejectedByAttr = false;
            curId = curYear = curTitle = curAnnotation = curAuthor = null;
        }

        while (await reader.ReadAsync())
        {
            ct.ThrowIfCancellationRequested();

            if (reader.NodeType == XmlNodeType.Element && reader.Name == query.MainNodeName)
            {
                Reset();
                inTarget = true;

                curId = reader.GetAttribute("id");
                curYear = reader.GetAttribute("year");

                if (!string.IsNullOrWhiteSpace(query.AttributeName) && query.AttributeValue != null)
                {
                    var attrVal = reader.GetAttribute(query.AttributeName);
                    if (!string.Equals(attrVal, query.AttributeValue, StringComparison.Ordinal))
                        rejectedByAttr = true;
                }
            }

            if (!inTarget) continue;

            if (rejectedByAttr)
            {
                // fast-skip subtree if possible
                if (reader.NodeType == XmlNodeType.EndElement && reader.Name == query.MainNodeName)
                    Reset();
                continue;
            }

            if (reader.NodeType == XmlNodeType.Element && reader.Name == "author")
            {
                curAuthor = reader.GetAttribute("fullName");
            }
            else if (reader.NodeType == XmlNodeType.Element && reader.Name == "title")
            {
                curTitle = await reader.ReadElementContentAsStringAsync();
            }
            else if (reader.NodeType == XmlNodeType.Element && reader.Name == "annotation")
            {
                curAnnotation = await reader.ReadElementContentAsStringAsync();
            }
            else if (reader.NodeType == XmlNodeType.EndElement && reader.Name == query.MainNodeName)
            {
                if (!string.IsNullOrWhiteSpace(query.Keyword))
                {
                    var kw = query.Keyword.Trim();
                    bool ok =
                        (curTitle?.Contains(kw, StringComparison.OrdinalIgnoreCase) ?? false) ||
                        (curAnnotation?.Contains(kw, StringComparison.OrdinalIgnoreCase) ?? false) ||
                        (curAuthor?.Contains(kw, StringComparison.OrdinalIgnoreCase) ?? false);

                    if (!ok)
                    {
                        Reset();
                        continue;
                    }
                }

                results.Add($"{curId ?? "-"} | {curYear ?? "-"} | {curAuthor ?? "(no author)"} — {curTitle ?? "(no title)"}");
                Reset();
            }
        }

        return results;
    }
}
