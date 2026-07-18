using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NovaPOS.Core.Interfaces.Services;

namespace NovaPOS.App.ViewModels.Products;

public partial class CategoryManagementViewModel : ObservableObject
{
    private readonly ICategoryService _categoryService;
    private readonly Action _close;

    public CategoryManagementViewModel(ICategoryService categoryService, Action close)
    {
        _categoryService = categoryService;
        _close = close;
        _ = LoadAsync();
    }

    public ObservableCollection<CategoryRowVm> Categories { get; } = [];

    [ObservableProperty]
    private string _newCategoryName = string.Empty;

    [ObservableProperty]
    private string? _statusMessage;

    [RelayCommand]
    private async Task LoadAsync()
    {
        var categories = await _categoryService.GetAllAsync();
        Categories.Clear();
        foreach (var category in categories)
        {
            var activeCount = await _categoryService.CountActiveProductsAsync(category.Id);
            Categories.Add(new CategoryRowVm(category.Id, category.Name, activeCount));
        }
    }

    [RelayCommand]
    private async Task AddAsync()
    {
        StatusMessage = null;
        if (string.IsNullOrWhiteSpace(NewCategoryName))
        {
            StatusMessage = "Enter a category name.";
            return;
        }

        try
        {
            await _categoryService.CreateAsync(NewCategoryName);
            NewCategoryName = string.Empty;
            await LoadAsync();
        }
        catch (Exception ex)
        {
            StatusMessage = ex.Message;
        }
    }

    [RelayCommand]
    private async Task RenameAsync(CategoryRowVm row)
    {
        StatusMessage = null;
        if (string.IsNullOrWhiteSpace(row.Name))
        {
            StatusMessage = "Category name cannot be empty.";
            return;
        }

        try
        {
            await _categoryService.RenameAsync(row.Id, row.Name);
            await LoadAsync();
        }
        catch (Exception ex)
        {
            StatusMessage = ex.Message;
        }
    }

    [RelayCommand]
    private async Task DeleteAsync(CategoryRowVm row)
    {
        StatusMessage = null;

        if (row.ActiveProductCount > 0)
        {
            StatusMessage = $"Cannot delete '{row.Name}' — {row.ActiveProductCount} active product(s) use this category.";
            return;
        }

        var confirm = MessageBox.Show(
            $"Delete category '{row.Name}'?",
            "Delete Category",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (confirm != MessageBoxResult.Yes)
        {
            return;
        }

        try
        {
            await _categoryService.DeleteAsync(row.Id);
            await LoadAsync();
        }
        catch (Exception ex)
        {
            StatusMessage = ex.Message;
        }
    }

    [RelayCommand]
    private void Close() => _close();
}

public partial class CategoryRowVm : ObservableObject
{
    public CategoryRowVm(Guid id, string name, int activeProductCount)
    {
        Id = id;
        Name = name;
        ActiveProductCount = activeProductCount;
    }

    public Guid Id { get; }

    [ObservableProperty]
    private string _name;

    public int ActiveProductCount { get; }
}
