using System;
using Android.Provider;
using FoodApp.IRepository;
using SQLite;

namespace FoodApp.Models;

public record BaseModel : IBaseModel
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
}

