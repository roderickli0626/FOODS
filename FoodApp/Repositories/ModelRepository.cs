using System;
using FoodApp.IRepository;
using FoodApp.Models;
using SQLite;

namespace FoodApp.Repositories;


public class ModelRepository : IModelRepository
{
    const string DB_NAME = "DB.db";

    readonly SQLiteAsyncConnection _asyncConnection;

    SQLiteConnectionString _dbConnectionString;

    readonly Task _InitialisationTask;
    readonly IMutexHolder _mutexHolder;

    public ModelRepository(IMutexHolder mutexHolder)
    {
        _mutexHolder = mutexHolder;

        _dbConnectionString = new SQLiteConnectionString(
            Path.Combine(FileSystem.AppDataDirectory, DB_NAME),
            SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.FullMutex | SQLiteOpenFlags.PrivateCache,
            false, null, null, null, null, "o", true
         );

        _asyncConnection = new SQLiteAsyncConnection(_dbConnectionString);

        // This hangs unit tests
        _InitialisationTask = CreateTables();
    }

    async Task CreateTables()
    {
        await _asyncConnection.CreateTableAsync<ProductItem>();
    }

    public async Task ResetTables()
    {
        await _InitialisationTask;

        await _asyncConnection.DeleteAllAsync<ProductItem>();
    }

    public async Task<TModel> GetByIdAsync<TModel>(int id, bool recursive = false)
            where TModel : class, IBaseModel, new()
    {
        await _InitialisationTask;

        TModel retVal;

        await _mutexHolder.AcquireMutexIfNeededAsync();

        try
        {
            retVal = await _asyncConnection.FindAsync<TModel>(id);
        }
        finally
        {
            _mutexHolder.ReleaseMutex();
        }

        return retVal;
    }

    public async Task InsertAsync(object element)
    {
        await _mutexHolder.AcquireMutexIfNeededAsync();

        try
        {
            await _asyncConnection.InsertAsync(element);
        }
        finally
        {
            _mutexHolder.ReleaseMutex();
        }
    }

    public async Task UpdateAsync(object element)
    {
        await _mutexHolder.AcquireMutexIfNeededAsync();

        try
        {
            await _asyncConnection.UpdateAsync(element);
        }
        finally
        {
            _mutexHolder.ReleaseMutex();
        }
    }

    public async Task DeleteAsync(object element)
    {
        try
        {
            await _asyncConnection.DeleteAsync(element);
        }
        finally
        {
            _mutexHolder.ReleaseMutex();
        }
    }
    
    public async Task<List<TModel>> QueryGetAsync<TModel>() where TModel : class, IBaseModel, new()
    {
        List<TModel> retVal;
        await _mutexHolder.AcquireMutexIfNeededAsync();

        try
        {
            var query = "Select * from ProductItem";
            retVal = await _asyncConnection.QueryAsync<TModel>(query);
        }
        finally
        {
            _mutexHolder.ReleaseMutex();
        }

        return retVal;
    }
}