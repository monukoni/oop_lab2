using System.Xml;
using System.Xml.Xsl;

namespace XmlLibraryLab2.Services;

public interface IXmlTransformService
{
    Task<string> TransformAsync(string xmlPath, string xslPath, string outputHtmlPath, CancellationToken ct);
}

public sealed class XsltTransformService : IXmlTransformService
{
    public Task<string> TransformAsync(string xmlPath, string xslPath, string outputHtmlPath, CancellationToken ct)
    {
        var xslt = new XslCompiledTransform();
        xslt.Load(xslPath);

        Directory.CreateDirectory(Path.GetDirectoryName(outputHtmlPath)!);

        using var writer = XmlWriter.Create(outputHtmlPath, xslt.OutputSettings);
        xslt.Transform(xmlPath, writer);

        return Task.FromResult(outputHtmlPath);
    }
}