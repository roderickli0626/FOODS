using ExpiryReminder.IRepository;
using ExpiryReminder.Models;
using ExpiryReminder.ViewModel;

namespace ExpiryReminder;

public partial class ProductDetailsPage : ContentPage
{
    ProductDetailsViewModel viewModel;
    public ProductDetailsPage(IModelRepository repository)
    {
        InitializeComponent();
        viewModel = new ProductDetailsViewModel(repository);
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        await viewModel.LoadData();

        base.OnAppearing();
    }

    private async void OnCollectionViewSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection != null && e.CurrentSelection.Count > 0)
        {
            // Handle the selection change, e.g., navigate to a details page for the selected item
            var selectedProduct = (ProductItem)e.CurrentSelection[0];
            await DisplayAlert(selectedProduct.Name, selectedProduct.Description, "Okay");
        }
    }

    private async void OnDeleteIconTapped(object sender, EventArgs e)
    {
        if (sender is Microsoft.Maui.Controls.Image deleteIcon && deleteIcon.BindingContext is ProductItem productItem)
        {
            bool answer = await DisplayAlert("Delete Entry", "Are you sure?", "Yes", "No");

            if (answer)
            {
                await viewModel.DeleteProduct(productItem.Id);
            }
        }
        // Handle delete icon tapped event, retrieve the ProductItem associated with the tapped icon, and delete it
    }

    async void ClickedCancel(System.Object sender, System.EventArgs e)
    {
        await Shell.Current.Navigation.PopToRootAsync(true);
    }

    private async void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
    {
        if (sender is Microsoft.Maui.Controls.Image editIcon && editIcon.BindingContext is ProductItem productItem)
        {
            await Shell.Current.Navigation.PushAsync(new ProductEditPage(productItem));
        }
    }
}
