using System;
namespace FoodApp.IRepository;

public interface IMediaService
{
    Task<string> SaveImageToAndroidGalleryAsync(string imagePath, string imageName);

    Task<string> SaveImageAsync(byte[] imageBytes, string fileName);
}

