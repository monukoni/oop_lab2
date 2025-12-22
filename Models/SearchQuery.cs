namespace XmlLibraryLab2.Models;

public sealed record SearchQuery(
    string MainNodeName,
    string? AttributeName,
    string? AttributeValue,
    string? Keyword
);