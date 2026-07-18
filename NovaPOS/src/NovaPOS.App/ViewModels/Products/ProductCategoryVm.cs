using CommunityToolkit.Mvvm.ComponentModel;
using NovaPOS.Core.Entities;

namespace NovaPOS.App.ViewModels.Products;

public partial class ProductCategoryVm : ObservableObject
{
    public ProductCategoryVm(Category category)
    {
        Id = category.Id;
        Name = category.Name;
        IsActive = category.IsActive;
    }

    public ProductCategoryVm(Guid id, string name)
    {
        Id = id;
        Name = name;
        IsActive = true;
    }

    public Guid Id { get; }
    public string Name { get; }
    public bool IsActive { get; }

    public override string ToString() => Name;
}
