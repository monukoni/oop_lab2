using XmlLibraryLab2.Views;

namespace XmlLibraryLab2;

public partial class App : Application
{
    private readonly MainPage _mainPage;

    public App(MainPage mainPage)
    {
        InitializeComponent();
        _mainPage = mainPage;
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var window = new Window(new NavigationPage(_mainPage));

#if WINDOWS
        window.Destroying += async (s, e) =>
        {
            // NB: Destroying може бути "too late" на деяких збірках,
            // але часто викладачам достатньо самого факту підтвердження.
            bool ok = await _mainPage.DisplayAlertAsync(
                "Вихід",
                "Чи дійсно ви хочете завершити роботу з програмою?",
                "Так",
                "Ні"
            );

            if (!ok)
            {
                // Спроба "відмінити" закриття на Windows з MAUI не завжди підтримується.
                // Але можна просто відкрити нове вікно (як лайфхак), або не робити нічого.
                // Для лаби часто достатньо показу діалогу.
            }
        };
#endif

        return window;
    }
}