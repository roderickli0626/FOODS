using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using FoodApp.IRepository;
using FoodApp.Models;

namespace FoodApp.ViewModel;

public class ProductDetailsViewModel : INotifyPropertyChanged
{
    IModelRepository _repository;
    ObservableCollection<ProductItem> _products;

    public ObservableCollection<ProductItem> Products
    {
        get => _products;
        set
        {
            _products = value;
            OnPropertyChanged(nameof(Products));
        }
    }

    public ProductDetailsViewModel(IModelRepository repository)
    {
        _repository = repository;
        Products = new();
    }

    // Initialize and populate the Products collection with data from the database
    public async Task LoadData()
    {
        var _productsItems = await _repository.QueryGetAsync<ProductItem>();
        Products = new ObservableCollection<ProductItem>(_productsItems);
    }

    public async Task DeleteProduct(int id)
    {
        var itemToDelete = Products.Where(x => x.Id == id).FirstOrDefault();
        await _repository.DeleteAsync(itemToDelete);

        Products.Remove(itemToDelete);
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}