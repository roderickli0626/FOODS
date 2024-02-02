using System;
namespace FoodApp.IRepository;

public interface IMutexHolder
{
    void AcquireMutexIfNeeded();
    Task AcquireMutexIfNeededAsync();
    void ReleaseMutex();
}
