using CommunityToolkit.Mvvm.ComponentModel;
using AivoraPOS.Core.Entities;

namespace AivoraPOS.App.ViewModels.Sales;

public partial class CategoryVm : ObservableObject
{
    public CategoryVm(Category category)
    {
        Id = category.Id;
        Name = category.Name;
    }

    public CategoryVm(Guid allId, string name)
    {
        Id = allId;
        Name = name;
    }

    public Guid Id { get; }
    public string Name { get; }
}
