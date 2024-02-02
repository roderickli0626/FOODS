namespace FoodApp.Models;

public record ProductDetails
{
    public string Description { get; set; }
    public List<string> Images { get; set; }
    public string Title { get; set; }
}

public record Root
{
    public ProductDetails Product { get; set; }
}