using XmlLibraryLab2.Strategies;

namespace XmlLibraryLab2.Services;

public interface IStrategyFactory
{
    IReadOnlyList<string> GetStrategyNames();
    IXmlSearchStrategy GetByName(string name);
}

public sealed class StrategyFactory : IStrategyFactory
{
    private readonly IReadOnlyList<IXmlSearchStrategy> _strategies;

    public StrategyFactory(IEnumerable<IXmlSearchStrategy> strategies)
    {
        _strategies = strategies.ToList();
    }

    public IReadOnlyList<string> GetStrategyNames()
        => _strategies.Select(s => s.Name).OrderBy(x => x).ToList();

    public IXmlSearchStrategy GetByName(string name)
        => _strategies.First(s => s.Name == name);
}