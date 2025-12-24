using System.Xml.Linq;

namespace XmlLibraryLab2.Services;

public interface IXmlMetadataService
{
    Task<IReadOnlyList<string>> GetAttributesAsync(string xmlPath, string mainNodeName, CancellationToken ct);
    Task<IReadOnlyList<string>> GetAttributeValuesAsync(string xmlPath, string mainNodeName, string attributeName, CancellationToken ct);
}

public sealed class XmlMetadataService : IXmlMetadataService
{
    public Task<IReadOnlyList<string>> GetAttributesAsync(string xmlPath, string mainNodeName, CancellationToken ct)
    {
        var doc = XDocument.Load(xmlPath);
        var nodes = doc.Descendants(mainNodeName);

        var attrs = new HashSet<string>(StringComparer.Ordinal);
        foreach (var n in nodes)
        {
            foreach (var a in n.Attributes())
                attrs.Add(a.Name.LocalName);
        }

        var result = attrs.OrderBy(x => x).ToList();
        return Task.FromResult<IReadOnlyList<string>>(result);
    }

    public Task<IReadOnlyList<string>> GetAttributeValuesAsync(string xmlPath, string mainNodeName, string attributeName, CancellationToken ct)
    {
        var doc = XDocument.Load(xmlPath);
        var nodes = doc.Descendants(mainNodeName);

        var values = nodes
            .Select(n => (string?)n.Attribute(attributeName))
            .Where(v => !string.IsNullOrWhiteSpace(v))
            .Distinct(StringComparer.Ordinal)
            .OrderBy(x => x)
            .ToList();

        return Task.FromResult<IReadOnlyList<string>>(values);
    }
}