namespace FoodApp.IRepository;

public interface IModelRepository
{
    Task ResetTables();

    Task<TModel> GetByIdAsync<TModel>(int id, bool recursive = false) where TModel : class, IBaseModel, new();

    Task InsertAsync(object element);

    Task UpdateAsync(object element);

    Task DeleteAsync(object element);
    
    Task<List<TModel>> QueryGetAsync<TModel>() where TModel : class, IBaseModel, new();
}