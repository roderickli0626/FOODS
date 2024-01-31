using System;
using ExpiryReminder.IRepository;

namespace ExpiryReminder.Repositories;

public class MutexHolder : IMutexHolder
{
    public static readonly SemaphoreSlim TheMutex = new SemaphoreSlim(1, 1);

    public MutexHolder()
    {
    }

    public void AcquireMutexIfNeeded()
    {
        {
            TheMutex.Wait();
        }
    }

    public async Task AcquireMutexIfNeededAsync()
    {
        {
            await TheMutex.WaitAsync();
        }
    }

    public void ReleaseMutex()
    {
        if (TheMutex.CurrentCount == 0)
        {
            TheMutex.Release();
        }
    }
}