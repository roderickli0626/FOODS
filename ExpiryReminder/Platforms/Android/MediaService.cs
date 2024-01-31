using System;
using Android.Content;
using Android.Provider;
using ExpiryReminder.IRepository;

namespace ExpiryReminder.Platforms.AndroidOS;

public class MediaService : IMediaService
{
    public async Task<string> SaveImageToAndroidGalleryAsync(string imagePath, string imageName)
    {
        var contentResolver = Android.App.Application.Context.ContentResolver;
        var contentValues = new ContentValues();
        contentValues.Put(MediaStore.Images.Media.InterfaceConsts.Title, imageName);
        contentValues.Put(MediaStore.Images.Media.InterfaceConsts.Description, "My app image");
        contentValues.Put(MediaStore.Images.Media.InterfaceConsts.MimeType, "image/png");

        string path = MediaStore.Images.Media.ExternalContentUri.ToString() + "/" + imageName + ".png"; 
        var stream = File.OpenRead(imagePath);
        var uri = contentResolver.Insert(MediaStore.Images.Media.ExternalContentUri, contentValues);

        using (var outputStream = contentResolver.OpenOutputStream(uri))
        {
            await stream.CopyToAsync(outputStream);
        }

        stream.Close();

        // This is the saved image path in the gallery
        return path; 
    }

    public async Task<string> SaveImageAsync(byte[] imageBytes, string fileName)
    {
        try
        {
            // Get the path to the app's data directory
            string dataDirPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);

            // Combine the data directory path with the file name to create the full file path
            string filePath = Path.Combine(dataDirPath, fileName);

            // Write the image bytes to the file
            using (FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                await fs.WriteAsync(imageBytes, 0, imageBytes.Length);
            }

            // Return the file path
            return filePath;
        }
        catch (Exception ex)
        {
            // Handle any exceptions
            Console.WriteLine($"Error saving image: {ex.Message}");
            return null;
        }
    }
}

