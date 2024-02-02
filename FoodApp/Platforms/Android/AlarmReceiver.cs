using System;
using Android.App;
using Android.Content;
using Android.Media;
using Android.OS;
using AndroidX.Core.App;

namespace FoodApp.Platforms;

[BroadcastReceiver(Exported = true, Enabled = true)]
public class AlarmReceiver : BroadcastReceiver
{
    public override async void OnReceive(Context context, Intent intent)
    {
        NotificationCompat.Builder builder = new NotificationCompat.Builder(context);

        builder.SetAutoCancel(true)
            .SetDefaults((int)NotificationDefaults.All)
            .SetSmallIcon(Android.Resource.Drawable.IcMenuReportImage)
            .SetContentTitle("Alarm Actived!")
            .SetContentText("test")
            .SetContentInfo("Info");

        var vibrator = (Vibrator)context.GetSystemService(Context.VibratorService);
        if (vibrator.HasVibrator)
        {
            builder.SetVibrate(new long[] { 500, 1000, 500 }); // Vibration pattern with delays
        }

        builder.SetSound(RingtoneManager.GetDefaultUri(RingtoneType.Alarm));
        //if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
        //{
        var channel = new NotificationChannel("channel_with_sound", "Channel with Sound", NotificationImportance.Max);

        channel.SetSound(Android.Net.Uri.Parse
            ("android.resource://" +
            Platform.AppContext.PackageName +
            "/raw/notification"),
                new AudioAttributes.Builder().SetUsage(AudioUsageKind.Notification).Build());

        //}

        NotificationManager manager = (NotificationManager)context.GetSystemService(Context.NotificationService);

        manager?.CreateNotificationChannel(channel);
        manager.Notify(1, builder.Build());
    }

    private void CreateNotificationChannel()
    {
        // Create a notification channel for Oreo and above
        if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
        {
            var channel = new NotificationChannel("channel_with_sound", "Channel with Sound", NotificationImportance.Max);

            channel.SetSound(Android.Net.Uri.Parse
                ("android.resource://" +
                Platform.AppContext.PackageName +
                "/raw/notification"),
                    new AudioAttributes.Builder().SetUsage(AudioUsageKind.Notification).Build());

            var notificationManager = Platform.AppContext.GetSystemService(Context.NotificationService) as NotificationManager;
            notificationManager?.CreateNotificationChannel(channel);
        }
    }

    private string GetNotificationSound()
    {
        if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
        {
            // For Android Oreo and above, use channel with sound
            CreateNotificationChannel();
            return "channel_with_sound";
        }
        else
        {
            // For Android versions below Oreo, use direct sound file
            return "android.resource://" + Platform.AppContext.PackageName + "/raw/notification";
        }
    }

}