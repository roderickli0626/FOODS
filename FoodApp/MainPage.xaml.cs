using System.Globalization;
using System.Text.RegularExpressions;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;
using Azure;
using Azure.AI.Vision.ImageAnalysis;
using FoodApp.IRepository;
using FoodApp.Models;
using FoodApp.Platforms;
using Newtonsoft.Json;
using Plugin.LocalNotification;
using Plugin.LocalNotification.EventArgs;
using Plugin.Maui.Audio;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using AndroidX.Navigation;
using Android.Content.Res;
using Android.Hardware;

namespace FoodApp;

public partial class MainPage : ContentPage
{
    //private readonly string RapidApiKey = "Enter your rapid api key";
    private readonly string RapidApiKey = "0968c01ab5msh67ae789ec54e7e9p19bdc2jsn0a038d7f12f0";

    protected IModelRepository _repository;
    private IAudioManager _audioManager;
    private IMediaService _mediaService;
    IAudioPlayer player;

    string imagePath = "empty.jpg";

    public MainPage( IModelRepository repository, IAudioManager audioManager, IMediaService mediaService)
    {
        InitializeComponent();
        HideBarcodeView(true);
        _repository = repository;
        _audioManager = audioManager;
        _mediaService = mediaService;

        InitializeTone();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        if (player.IsPlaying)
        {
            player.Stop();
        }

        if (App.IsPreview)
        {
            Shell.Current.GoToAsync(nameof(PreviewPage));
        }
    }

    private async void InitializeTone()
    {
        player = _audioManager.CreatePlayer(await FileSystem.OpenAppPackageFileAsync("alert.mp3"));
    }

    public async Task<ProductDetails> GetProductDetailsAsync(string barcodeValue, bool retry = true)
    {
        ProductDetails productDetails = null;
        try
        {
            var client = new HttpClient();
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri($"https://barcodes1.p.rapidapi.com/?query={barcodeValue}"),
                Headers =
                {
                    { "X-RapidAPI-Key",  RapidApiKey},
                    { "X-RapidAPI-Host", "barcodes1.p.rapidapi.com" },
                },
            };

            using (var response = await client.SendAsync(request))
            {
                response.EnsureSuccessStatusCode();
                var body = await response.Content.ReadAsStringAsync();

                if (!string.IsNullOrEmpty(body))
                    productDetails = (JsonConvert.DeserializeObject<Root>(body))?.Product;
            }

            var isImageAvailable = productDetails?.Images?.FirstOrDefault().Any() ?? false;
            if (isImageAvailable)
            {
                try
                {
                    var imageData = await client.GetByteArrayAsync(productDetails.Images.FirstOrDefault());

                    imagePath = await _mediaService.SaveImageAsync(imageData, barcodeValue + DateTime.Now.Ticks);

                    var imageSource = ImageSource.FromStream(() => new System.IO.MemoryStream(imageData));
                }
                catch (Exception ex)
                {
                    imagePath = "empty.jpg";
                }
            }
            else
            {
                imagePath = "empty.jpg";
            }
            
            productImage.Source = imagePath;
        }
        catch (Exception ex)
        {
            imagePath = "empty.jpg";
            productImage.Source = imagePath;

            Console.WriteLine($"Exception: {ex.Message}");

            // If API fails, retry once again
            if (retry)
            {
                await Task.Delay(1000);
                productDetails = await GetProductDetailsAsync(barcodeValue, false);
            }
        }
        return productDetails;
    }

    private void ClickedBarcode(object sender, EventArgs e)
    {
        HideBarcodeView(false);
    }

    private Task<NotificationEventReceivingArgs> NotificationReceivingEvent(NotificationRequest request)
    {
        player.Loop = true;
        player.Play();
        btnRinger.IsVisible = true;
        return null;
    }

    private void NotificationActionTappedEvent(NotificationActionEventArgs e)
    {
        btnRinger.IsVisible = false;

        player.Stop();
    }

    static List<string> AnalyzeImage(byte[] path)
    {
        var AllLines = new List<string>();

        try
        {
            //string endpoint = "ENTER YOUR AZURE END-POINT";
            //string key = "ENTER AZURE SUBSCRIPTION KEY";

            string endpoint = "https://appfoddss.cognitiveservices.azure.com/";
            string key = "7aaf2248cd6b4000bace993baa8ab804";


            ImageAnalysisClient client = new ImageAnalysisClient(
                new Uri(endpoint),
                new AzureKeyCredential(key));

            System.BinaryData binaryData = new System.BinaryData(path);

            ImageAnalysisResult result = client.Analyze(
                binaryData, VisualFeatures.Read,
                new ImageAnalysisOptions { GenderNeutralCaption = true });

            foreach (DetectedTextBlock block in result?.Read?.Blocks)
            {
                var lines = block.Lines.Select(x => x.Text);
                AllLines.AddRange(lines);
            }
        }
        catch (Exception ex)
        {

        }
        return AllLines;
    }

    private async void ClickedExpiry(object sender, EventArgs e)
    {
        try
        {
            List<string> detectedLines = new();
            ProductExpiry.Text = "Scansione di Pic, attendere.";

            await DisplayAlert("Attenzione", "Per favore fai una foto alla data di scadenza del prodotto.", "Conferma");

            var file = await TakePhotoAsync();

            if (file == null)
                return;

            byte[] imageData = await File.ReadAllBytesAsync(file.FullPath);

            detectedLines = AnalyzeImage(imageData);

            if (detectedLines.Count < 1)
            {
                ProductExpiry.Text = "L'immagine catturata non era chiara!";
                return;
            }

            List<DateTime> expiryDates = ExtractExpiryDates(detectedLines);

            if (expiryDates.Count > 1)
            {
                ProductExpiry.Text = expiryDates.Max().ToString("dd-MM-yyyy");
            }
            else if (expiryDates.Count == 1)
            {
                ProductExpiry.Text = expiryDates.FirstOrDefault().ToString("dd-MM-yyyy");
            }
            else
            {
                ProductExpiry.Text = "Non è stato trovato un formato di data corretto";
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception occured in: {nameof(ClickedExpiry)} with the error message: {ex.Message}");
        }
    }

    private async void ClickedSave(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(ProductLabel.Text) ||
            string.IsNullOrWhiteSpace(ProductDescription.Text) ||
            string.IsNullOrWhiteSpace(ProductExpiry.Text))
        {
            await DisplayAlert("Attenzione", "Per favore inserisci tutti i dettagli prima di salvare.", "OK");
            return;
        }

        ProductItem item = new()
        {
            Name = ProductLabel.Text,
            Description = ProductDescription.Text,
            Barcode = ProductBarcode.Text,
            ExpiryDate = ProductExpiry.Text,
            ImagePath = imagePath,
            InsertDate = DateTime.Now.ToString("dd-MM-yyyy"),
            Price = string.IsNullOrEmpty(ProductPrice.Text) ? "0" : ProductPrice.Text
        };

        var _productsItems = await _repository.QueryGetAsync<ProductItem>();
        if (_productsItems.Count > 29)
        {
            await DisplayAlert("Attenzione", "Nella versione gratuita, è possibile memorizzare solo 30 prodotti. Per un numero maggiore di prodotti, è necessario acquistarne una.", "OK");
            return;
        }

        await _repository.InsertAsync(item);

        await SetNotificationForExpiry(item.ExpiryDate, item.Name);

        ProductLabel.Text =
        ProductDescription.Text =
        ProductBarcode.Text =
        ProductPrice.Text = 
        ProductExpiry.Text = "";

        imagePath = "empty.jpg";

        productImage.Source = "empty.jpg";
    }

    private async Task SetNotificationForExpiry(string expiryDate, string name)
    {
        if (!DateTime.TryParse(expiryDate, out DateTime date))
            return;

        if (date.Date <= DateTime.Now.Date)
            return;

        DateTime dateToNotify = date;

        dateToNotify = new DateTime(dateToNotify.Year, dateToNotify.Month, dateToNotify.Day, 11, 0, 0);

        //var filePath = Path.Combine(FileSystem.AppDataDirectory, "Resources", "Raw", "alert.mp3");

        var request = new NotificationRequest
        {
            NotificationId = 1000,
            Title = $"Prodotto: {name} Scadenza oggi!",
            Subtitle = "Attenzione...",
            Description = "Clicca per aprire",
            BadgeNumber = 1,

            Schedule = new NotificationRequestSchedule
            {
                NotifyTime = dateToNotify,
            }
        };

        LocalNotificationCenter.Current.NotificationActionTapped += NotificationActionTappedEvent;
        LocalNotificationCenter.Current.NotificationReceiving += NotificationReceivingEvent;

        await LocalNotificationCenter.Current.Show(request);

        try
        {
            Intent alarmIntent = new Intent(Android.App.Application.Context, typeof(AlarmReceiver));

            PendingIntent pendingIntent;

            //MauiApp Context: Microsoft.Maui.ApplicationModel.Platform.CurrentActivity
            if (Build.VERSION.SdkInt > BuildVersionCodes.Lollipop)
            {
                pendingIntent = PendingIntent.GetBroadcast(Android.App.Application.Context, 0, alarmIntent, PendingIntentFlags.Immutable);
            }
            else
            {
                pendingIntent = PendingIntent.GetBroadcast(Android.App.Application.Context, 0, alarmIntent, PendingIntentFlags.CancelCurrent);
            }

            AlarmManager alarmManager = (AlarmManager)Android.App.Application.Context.GetSystemService(Context.AlarmService);

            long triggerTime = Android.OS.SystemClock.ElapsedRealtime() + 10000; // 10 seconds
            alarmManager.Set(AlarmType.ElapsedRealtimeWakeup, triggerTime, pendingIntent);
        }
        catch (Exception ex)
        {

        }

    }

    private async void ClickedViewRecords(System.Object sender, System.EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(ProductDetailsPage));
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

    static List<DateTime> ExtractExpiryDates(List<string> inputTextList)
    {
        List<DateTime> expiryDates = new List<DateTime>();

        // Define regular expressions for common date formats
        string[] datePatterns =
        {
            @"(?<day>\d{1,2})[^\d]*(?<month>\d{1,2})[^\d]*(?<year>\d{2,4})",
            @"\b(?<month>\d{2})[\/\.-]?(?<year>\d{4})\b"
        };

        /* APPROVED DATE FORMATS FOR NOW

        MMDDYYYY
        MMDDYY
        DDMMYY
        DDMMYYYY
        YYYYMMDD
        YYYYDDMM

        MM DD YYYY
        MM DD YY
        DD MM YY
        DD MM YYYY
        YYYY DD MM
        YYYY MM DD

        DD-MM-YYYY
        DD-MM-YY
        MM-DD-YYYY
        MM-DD-YY
        YYYY-DD-MM
        YY-DD-MM
        YY-MM-DD
        YYYY-MM-DD

        DD/MM/YYYY
        DD/MM/YY
        MM/DD/YYYY
        MM/DD/YY
        YYYY/DD/MM
        YYYY/MM/DD
        YY/DD/MM
        YY/MM/DD

        DD.MM.YYYY
        DD.MM.YY
        MM.DD.YYYY
        MM.DD.YY
        YYYY.DD.MM
        YYYY.MM.DD
        YY.DD.MM
        YY.MM.DD

         */
        string[] formats = { "yyyy/dd/MM", "yyyy/d/MM", "yyyy/dd/M", "yyyy/d/M" };

        foreach(string inputText in inputTextList)
        {
            foreach (string pattern in datePatterns)
            {
                MatchCollection matches = Regex.Matches(inputText, pattern);

                foreach (Match match in matches)
                {
                    var parsedYear = int.Parse(match.Groups["year"].Value);

                    if (parsedYear < 100)
                        parsedYear += 2000;

                    if (parsedYear <= 2030 && parsedYear >= 2020)
                    {
                        string dateString;

                        if (string.IsNullOrWhiteSpace(match.Groups["day"]?.Value))
                        {
                            if (match.Groups["month"].Value == "2" || match.Groups["month"].Value == "02")
                            {
                                dateString = $"{parsedYear}/28/{match.Groups["month"].Value}";
                            }
                            else
                            {
                                dateString = $"{parsedYear}/30/{match.Groups["month"].Value}";
                            }
                        }
                        else
                        {
                            dateString = $"{parsedYear}/{match.Groups["day"].Value}/{match.Groups["month"].Value}";
                        }

                        if (DateTime.TryParseExact(dateString, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date))
                        {
                            expiryDates.Add(date);
                        }
                    }
                }
            }
        }

        return expiryDates;
    }

    void CameraView_BarcodeDetected(System.Object sender, Camera.MAUI.ZXingHelper.BarcodeEventArgs args)
    {
        var result = args.Result[0].Text;

        if (string.IsNullOrEmpty(result))
            return;

        MainThread.BeginInvokeOnMainThread(async () =>
        {
            var str = Regex.Replace(result, @"\s", "");
            ProductBarcode.Text = str;

            ProductLabel.Text = ProductDescription.Text = "Recupero dei dettagli...";

            HideBarcodeView(true);

            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            string text = "Aspettate. Le informazioni sul prodotto si stanno grattando...";
            ToastDuration duration = ToastDuration.Long;
            double fontSize = 18;
            var toast = CommunityToolkit.Maui.Alerts.Toast.Make(text, duration, fontSize);
            await toast.Show(cancellationTokenSource.Token);

            var productDetails = await GetProductDetailsAsync(str);

            if (productDetails != null)
            {
                if (string.IsNullOrWhiteSpace(productDetails?.Title))
                {
                    ProductLabel.Text = "Nessun dato trovato";
                }
                else
                {
                    ProductLabel.Text = productDetails?.Title;
                }

                if (string.IsNullOrWhiteSpace(productDetails?.Description))
                {
                    ProductDescription.Text = "Nessun dato trovato";
                }
                else
                {
                    ProductDescription.Text = productDetails?.Description;
                }
            }
            else
            {
                ProductLabel.Text = "Nessun dato trovato";
                ProductDescription.Text = "Nessun dato trovato";
            }

            
        });
    }

    void CameraView_CamerasLoaded(System.Object sender, System.EventArgs e)
    {
        if (cameraView.Cameras.Count > 0)
        {
            cameraView.Camera = cameraView.Cameras.First();
            MainThread.BeginInvokeOnMainThread(async () =>
                {
                    await Task.Delay(500);
                    await cameraView.StopCameraAsync();
                    await cameraView.StartCameraAsync();
                });
        }
    }

    void ClickedCancel(System.Object sender, System.EventArgs e)
    {
        HideBarcodeView(true, true);
    }

    void HideBarcodeView(bool showProductView, bool clearValues = false)
    {
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            try
            {
                if (showProductView)
                {
                    await cameraView?.StopCameraAsync();
                }
                else
                {
                    if (cameraView?.Cameras?.Count > 0)
                    {
                        cameraView.Camera = cameraView?.Cameras.First();
                        MainThread.BeginInvokeOnMainThread(async () =>
                        {
                            await Task.Delay(500);
                            await cameraView?.StopCameraAsync();
                            await cameraView?.StartCameraAsync();
                        });
                    }
                }
                stackViewBarcode.IsVisible = !showProductView;
                scrollViewData.IsVisible = showProductView;
            }
            catch (Exception ex)
            {
                ProductLabel.Text = ProductDescription.Text = "Errore nella scansione del codice a barre";
            }
        });
        
    }

    void ClickedStopRinging(System.Object sender, System.EventArgs e)
    {
        if (player.IsPlaying)
            player.Stop();

        btnRinger.IsVisible = false;

    }
}