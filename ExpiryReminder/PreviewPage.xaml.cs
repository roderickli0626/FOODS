namespace ExpiryReminder;

public partial class PreviewPage : ContentPage
{
	public PreviewPage()
	{
		InitializeComponent();
    }

    async void Button_Clicked(object sender, EventArgs e)
    {
		App.IsPreview = false;
		await Shell.Current.Navigation.PopToRootAsync(true);
    }
}