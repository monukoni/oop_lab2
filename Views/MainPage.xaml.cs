using XmlLibraryLab2.ViewModels;

namespace XmlLibraryLab2.Views;

public partial class MainPage : ContentPage
{
    public MainPage(MainViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}