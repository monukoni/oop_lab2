using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using XmlLibraryLab2.Models;
using XmlLibraryLab2.Services;

namespace XmlLibraryLab2.ViewModels;

public sealed class MainViewModel : INotifyPropertyChanged
{
    private readonly IXmlMetadataService _metadata;
    private readonly IXmlTransformService _transform;
    private readonly IStrategyFactory _factory;

    private CancellationTokenSource? _cts;

    public event PropertyChangedEventHandler? PropertyChanged;

    public MainViewModel(IXmlMetadataService metadata, IXmlTransformService transform, IStrategyFactory factory)
    {
        _metadata = metadata;
        _transform = transform;
        _factory = factory;

        StrategyNames = _factory.GetStrategyNames();
        SelectedStrategyName = StrategyNames.FirstOrDefault() ?? "";

        SelectXmlCommand = new Command(async () => await SelectXmlAsync());
        SelectXslCommand = new Command(async () => await SelectXslAsync());
        SearchCommand = new Command(async () => await SearchAsync(), () => CanSearch);
        ClearCommand = new Command(Clear);
        TransformCommand = new Command(async () => await TransformAsync(), () => CanTransform);
        OpenHtmlCommand = new Command(async () => await OpenHtmlAsync(), () => CanOpenHtml);

        MainNodeName = "book";
        XmlPath = "XML: (не вибрано)";
        XslPath = "XSL: (не вибрано)";
        Output = "";
        HtmlOutputPath = "";
        Keyword = "";

        AvailableAttributes = new List<string>();
        AvailableAttributeValues = new List<string>();
    }

    // === Properties ===
    public string MainNodeName { get; }

    private string _xmlPath = "";
    public string XmlPath
    {
        get => _xmlPath;
        set { _xmlPath = value; OnPropertyChanged(); RefreshCanExecute(); }
    }

    private string _xslPath = "";
    public string XslPath
    {
        get => _xslPath;
        set { _xslPath = value; OnPropertyChanged(); RefreshCanExecute(); }
    }

    private string _output = "";
    public string Output
    {
        get => _output;
        set { _output = value; OnPropertyChanged(); }
    }

    private string _keyword = "";
    public string Keyword
    {
        get => _keyword;
        set { _keyword = value; OnPropertyChanged(); UpdateQueryPreview(); }
    }

    public IReadOnlyList<string> StrategyNames { get; }

    private string _selectedStrategyName = "";
    public string SelectedStrategyName
    {
        get => _selectedStrategyName;
        set { _selectedStrategyName = value; OnPropertyChanged(); }
    }

    private List<string> _availableAttributes = new();
    public List<string> AvailableAttributes
    {
        get => _availableAttributes;
        set { _availableAttributes = value; OnPropertyChanged(); }
    }

    private string? _selectedAttribute;
    public string? SelectedAttribute
    {
        get => _selectedAttribute;
        set
        {
            _selectedAttribute = value;
            OnPropertyChanged();
            _ = LoadAttributeValuesAsync();
            UpdateQueryPreview();
        }
    }

    private List<string> _availableAttributeValues = new();
    public List<string> AvailableAttributeValues
    {
        get => _availableAttributeValues;
        set { _availableAttributeValues = value; OnPropertyChanged(); }
    }

    private string? _selectedAttributeValue;
    public string? SelectedAttributeValue
    {
        get => _selectedAttributeValue;
        set { _selectedAttributeValue = value; OnPropertyChanged(); UpdateQueryPreview(); }
    }

    private string _queryPreview = "";
    public string QueryPreview
    {
        get => _queryPreview;
        set { _queryPreview = value; OnPropertyChanged(); }
    }

    private string _htmlOutputPath = "";
    public string HtmlOutputPath
    {
        get => _htmlOutputPath;
        set
        {
            _htmlOutputPath = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(CanOpenHtml));
            (OpenHtmlCommand as Command)?.ChangeCanExecute();
        }
    }

    public bool CanOpenHtml => !string.IsNullOrWhiteSpace(HtmlOutputPath) && File.Exists(HtmlOutputPath);

    // === Commands ===
    public ICommand SelectXmlCommand { get; }
    public ICommand SelectXslCommand { get; }
    public ICommand SearchCommand { get; }
    public ICommand ClearCommand { get; }
    public ICommand TransformCommand { get; }
    public ICommand OpenHtmlCommand { get; }

    private bool HasXmlPicked => XmlPath.StartsWith("XML: ") && XmlPath.Length > 5 && !XmlPath.EndsWith("(не вибрано)");
    private bool HasXslPicked => XslPath.StartsWith("XSL: ") && XslPath.Length > 5 && !XslPath.EndsWith("(не вибрано)");

    private string? GetRealPath(string labelPath)
    {
        var idx = labelPath.IndexOf(": ", StringComparison.Ordinal);
        if (idx < 0) return null;
        var p = labelPath[(idx + 2)..].Trim();
        return File.Exists(p) ? p : null;
    }

    public bool CanSearch => GetRealPath(XmlPath) != null && !string.IsNullOrWhiteSpace(SelectedStrategyName);
    public bool CanTransform => GetRealPath(XmlPath) != null && GetRealPath(XslPath) != null;

    private void RefreshCanExecute()
    {
        (SearchCommand as Command)?.ChangeCanExecute();
        (TransformCommand as Command)?.ChangeCanExecute();
    }

    private async Task SelectXmlAsync()
    {
        try
        {
            var res = await FilePicker.Default.PickAsync(new PickOptions
            {
                PickerTitle = "Select XML file"
            });

            if (res == null) return;

            if (!res.FileName.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
            {
                await Shell.Current.DisplayAlertAsync("Info", "Обери файл .xml", "OK");
                return;
            }

            // Копіюємо у доступну директорію застосунку
            var localXml = await CopyPickedFileAsync(res, "input.xml");

            XmlPath = $"XML: {localXml}";
            Output = "";
            await LoadAttributesAsync();
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Error", ex.ToString(), "OK");
        }
    }


    
    private async Task SelectXslAsync()
    {
        try
        {
            var res = await FilePicker.Default.PickAsync(new PickOptions
            {
                PickerTitle = "Select XSL file"
            });

            if (res == null) return;

            if (!res.FileName.EndsWith(".xsl", StringComparison.OrdinalIgnoreCase) &&
                !res.FileName.EndsWith(".xslt", StringComparison.OrdinalIgnoreCase))
            {
                await Shell.Current.DisplayAlertAsync("Info", "Обери файл .xsl або .xslt", "OK");
                return;
            }

            var localXsl = await CopyPickedFileAsync(res, "transform.xsl");

            XslPath = $"XSL: {localXsl}";
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Error", ex.ToString(), "OK");
        }
    }



    private async Task LoadAttributesAsync()
    {
        var xml = GetRealPath(XmlPath);
        if (xml == null) return;

        _cts?.Cancel();
        _cts = new CancellationTokenSource();

        try
        {
            var attrs = await _metadata.GetAttributesAsync(xml, MainNodeName, _cts.Token);
            AvailableAttributes = attrs.ToList();

            SelectedAttribute = AvailableAttributes.FirstOrDefault(); // авто-вибір
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", ex.Message, "OK");
        }
    }

    private async Task LoadAttributeValuesAsync()
    {
        var xml = GetRealPath(XmlPath);
        if (xml == null) return;

        if (string.IsNullOrWhiteSpace(SelectedAttribute))
        {
            AvailableAttributeValues = new List<string>();
            SelectedAttributeValue = null;
            return;
        }

        _cts?.Cancel();
        _cts = new CancellationTokenSource();

        try
        {
            var vals = await _metadata.GetAttributeValuesAsync(xml, MainNodeName, SelectedAttribute!, _cts.Token);
            AvailableAttributeValues = vals.ToList();

            SelectedAttributeValue = AvailableAttributeValues.FirstOrDefault(); // авто-вибір
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", ex.Message, "OK");
        }
    }

    private SearchQuery BuildQuery()
        => new SearchQuery(
            MainNodeName,
            string.IsNullOrWhiteSpace(SelectedAttribute) ? null : SelectedAttribute,
            string.IsNullOrWhiteSpace(SelectedAttributeValue) ? null : SelectedAttributeValue,
            string.IsNullOrWhiteSpace(Keyword) ? null : Keyword.Trim()
        );

    private void UpdateQueryPreview()
    {
        // “динамічна генерація запиту” (preview як користувачу)
        // Показуємо умовний XPath-подібний запит
        var parts = new List<string> { $"//{MainNodeName}" };

        if (!string.IsNullOrWhiteSpace(SelectedAttribute) && !string.IsNullOrWhiteSpace(SelectedAttributeValue))
            parts.Add($"[@{SelectedAttribute}='{SelectedAttributeValue}']");

        if (!string.IsNullOrWhiteSpace(Keyword))
            parts.Add($"[contains(title|annotation|author/@fullName, '{Keyword.Trim()}')]");

        QueryPreview = string.Join("", parts);
    }

    private async Task SearchAsync()
    {
        var xml = GetRealPath(XmlPath);
        if (xml == null)
        {
            await Shell.Current.DisplayAlert("Info", "Спочатку вибери XML", "OK");
            return;
        }

        var query = BuildQuery();
        var strategy = _factory.GetByName(SelectedStrategyName);

        try
        {
            Output = "Searching...\n";
            var res = await strategy.SearchAsync(xml, query, CancellationToken.None);

            if (res.Count == 0)
                Output = $"[{strategy.Name}] Нічого не знайдено.\n";
            else
                Output = $"[{strategy.Name}] Found: {res.Count}\n\n" + string.Join("\n", res);
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", ex.Message, "OK");
        }
    }

    private void Clear()
    {
        Keyword = "";
        Output = "";
        HtmlOutputPath = "";
        // параметри пошуку:
        if (AvailableAttributes.Count > 0) SelectedAttribute = AvailableAttributes[0];
        if (AvailableAttributeValues.Count > 0) SelectedAttributeValue = AvailableAttributeValues[0];
        UpdateQueryPreview();
    }

    private async Task TransformAsync()
    {
        var xml = GetRealPath(XmlPath);
        var xsl = GetRealPath(XslPath);

        if (xml == null || xsl == null)
        {
            await Alert("Info", "Вибери XML і XSL");
            return;
        }

        if (!File.Exists(xml))
        {
            await Alert("Error", $"XML не знайдено:\n{xml}");
            return;
        }

        if (!File.Exists(xsl))
        {
            await Alert("Error", $"XSL не знайдено:\n{xsl}");
            return;
        }

        try
        {
            var outDir = Path.Combine(FileSystem.AppDataDirectory, "html");
            Directory.CreateDirectory(outDir);

            var outPath = Path.Combine(outDir, "library.html");

            var html = await _transform.TransformAsync(xml, xsl, outPath, CancellationToken.None);

            HtmlOutputPath = html;
            await Alert("OK", $"HTML створено:\n{html}");
        }
        catch (Exception ex)
        {
            await Alert("Error", ex.ToString());
        }
    }

    private static Page UiPage =>
        Application.Current?.Windows.FirstOrDefault()?.Page
        ?? Application.Current?.MainPage
        ?? throw new InvalidOperationException("UI page is not available yet.");

    private static Task Alert(string title, string msg)
        => UiPage.DisplayAlertAsync(title, msg, "OK");

    private static Task<bool> Confirm(string title, string msg)
        => UiPage.DisplayAlertAsync(title, msg, "Так", "Ні");

    
    private async Task OpenHtmlAsync()
    {
        if (!CanOpenHtml) return;

        try
        {
            await Launcher.Default.OpenAsync(new OpenFileRequest
            {
                File = new ReadOnlyFile(HtmlOutputPath)
            });
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", ex.Message, "OK");
        }
    }
    
    private static async Task<string> CopyPickedFileAsync(FileResult file, string targetFileName)
    {
        var dir = Path.Combine(FileSystem.AppDataDirectory, "picked");
        Directory.CreateDirectory(dir);

        var destPath = Path.Combine(dir, targetFileName);

        await using var src = await file.OpenReadAsync();
        await using var dst = File.Create(destPath);
        await src.CopyToAsync(dst);

        return destPath;
    }


    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
