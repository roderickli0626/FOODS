namespace FoodApp.Models;

public record ProductItem : BaseModel
{
    public string Name { get; set; }
    public string ImagePath { get; set; }
    public string Description { get; set; }
    public string Barcode { get; set; }
    public string ExpiryDate { get; set; }
    public string InsertDate { get; set; }
    public string Price { get; set; }
}