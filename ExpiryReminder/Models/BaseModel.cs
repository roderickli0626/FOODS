using System;
using Android.Provider;
using ExpiryReminder.IRepository;
using SQLite;

namespace ExpiryReminder.Models;

public record BaseModel : IBaseModel
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
}

