namespace FoodApp;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();

        MainPage = new AppShell();
    }

    public static bool IsPreview = true;
}

