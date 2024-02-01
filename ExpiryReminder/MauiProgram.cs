using Camera.MAUI;
using CommunityToolkit.Maui;
using ExpiryReminder.IRepository;
using ExpiryReminder.Platforms.AndroidOS;
using ExpiryReminder.Repositories;
using Microsoft.Extensions.Logging;
using Plugin.LocalNotification;
using Plugin.Maui.Audio;
using TesseractOcrMaui;

namespace ExpiryReminder;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .UseLocalNotification()
            .UseMauiCameraView()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        builder.Services.AddSingleton(AudioManager.Current);

#if DEBUG
        builder.Logging.AddDebug();
#endif

        builder.Services.AddSingleton<IModelRepository, ModelRepository>();
        builder.Services.AddSingleton<IMutexHolder, MutexHolder>();
        builder.Services.AddSingleton<IMediaService, MediaService>();


        builder.Services.AddTransient<MainPage>();
        builder.Services.AddTransient<ProductDetailsPage>();
        builder.Services.AddTransient<PreviewPage>();

        return builder.Build();
    }
}