using FoodApp.IRepository;
using FoodApp.Models;
using FoodApp.Platforms.AndroidOS;
using FoodApp.Repositories;
using Plugin.Maui.Audio;
using ZXing.PDF417.Internal;
using static Android.Content.ClipData;

namespace FoodApp;

public partial class ProductEditPage : ContentPage
{
    private ProductItem productItem;

    protected IModelRepository _repository;
    private IAudioManager _audioManager;
    private IMediaService _mediaService;

    private string imagePath = "";
    public ProductEditPage(ProductItem proItem)
	{
		InitializeComponent();

        this.productItem = proItem;

        _repository = new ModelRepository(new MutexHolder());
        _audioManager = new AudioManager();
        _mediaService = new MediaService();


        productImage.Source = productItem.ImagePath;
		ProductLabel.Text = productItem.Name;
		ProductBarcode.Text = productItem.Barcode;
		ProductDescription.Text = productItem.Description;
		ProductExpiry.Text = productItem.ExpiryDate;
		ProductPrice.Text = productItem.Price;
	}

    async void Button_Clicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(ProductLabel.Text) ||
            string.IsNullOrWhiteSpace(ProductDescription.Text) ||
            string.IsNullOrWhiteSpace(ProductExpiry.Text))
        {
            await DisplayAlert("Attenzione", "Per favore inserisci tutti i dettagli prima di salvare.", "OK");
            return;
        }

        productItem.Name = ProductLabel.Text;
        productItem.Barcode = ProductBarcode.Text;
        productItem.Description = ProductDescription.Text;
        productItem.Price = ProductPrice.Text;
        productItem.ExpiryDate = ProductExpiry.Text;
        if (imagePath != "") productItem.ImagePath = imagePath;

        await _repository.UpdateAsync(productItem);

        await Shell.Current.Navigation.PopAsync();
    }

    async void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
    {
        var file = await TakePhotoAsync();

        if (file == null)
            return;

        byte[] imageData = await File.ReadAllBytesAsync(file.FullPath);

        imagePath = await _mediaService.SaveImageAsync(imageData, "" + DateTime.Now.Ticks);

        productImage.Source = imagePath;
    }

    private async Task<FileResult> TakePhotoAsync()
    {
        try
        {
            var photoResult = await MediaPicker.Default.CapturePhotoAsync(new MediaPickerOptions
            {
                Title = "Capture Photo"
            });

            if (photoResult != null)
            {
                return photoResult;
            }
            else
            {
                // User canceled taking a photo
                await DisplayAlert("Cancellato", "Lo scatto della foto è stato annullato", "OK");
            }
        }
        catch (Exception ex)
        {
            // Handle exception
        }
        return null;
    }
}