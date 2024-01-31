using System;
namespace ExpiryReminder.IRepository;

public interface IMutexHolder
{
    void AcquireMutexIfNeeded();
    Task AcquireMutexIfNeededAsync();
    void ReleaseMutex();
}
