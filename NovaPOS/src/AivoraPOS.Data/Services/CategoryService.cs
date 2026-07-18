using Microsoft.Extensions.Logging;
using AivoraPOS.Core.Entities;
using AivoraPOS.Core.Interfaces.Repositories;
using AivoraPOS.Core.Interfaces.Services;

namespace AivoraPOS.Data.Services;

public sealed class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IProductRepository _productRepository;
    private readonly ILogger<CategoryService> _logger;

    public CategoryService(
        ICategoryRepository categoryRepository,
        IProductRepository productRepository,
        ILogger<CategoryService> logger)
    {
        _categoryRepository = categoryRepository;
        _productRepository = productRepository;
        _logger = logger;
    }

    public Task<IReadOnlyList<Category>> GetAllAsync(CancellationToken cancellationToken = default) =>
        _categoryRepository.GetAllOrderedAsync(cancellationToken);

    public async Task<Category> CreateAsync(string name, CancellationToken cancellationToken = default)
    {
        var normalized = name.Trim();
        if (string.IsNullOrWhiteSpace(normalized))
        {
            throw new InvalidOperationException("Category name is required.");
        }

        var existing = await _categoryRepository.GetByNameAsync(normalized, cancellationToken);
        if (existing is not null)
        {
            throw new InvalidOperationException($"Category '{normalized}' already exists.");
        }

        var category = new Category
        {
            Name = normalized,
            IsActive = true
        };

        await _categoryRepository.AddAsync(category, cancellationToken);
        await _categoryRepository.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Created category {CategoryName}.", category.Name);
        return category;
    }

    public async Task<Category> RenameAsync(Guid categoryId, string name, CancellationToken cancellationToken = default)
    {
        var normalized = name.Trim();
        if (string.IsNullOrWhiteSpace(normalized))
        {
            throw new InvalidOperationException("Category name is required.");
        }

        var category = await _categoryRepository.GetByIdAsync(categoryId, cancellationToken)
            ?? throw new InvalidOperationException("Category was not found.");

        var duplicate = await _categoryRepository.GetByNameAsync(normalized, cancellationToken);
        if (duplicate is not null && duplicate.Id != categoryId)
        {
            throw new InvalidOperationException($"Category '{normalized}' already exists.");
        }

        category.Name = normalized;
        await _categoryRepository.UpdateAsync(category, cancellationToken);
        await _categoryRepository.SaveChangesAsync(cancellationToken);
        return category;
    }

    public async Task DeleteAsync(Guid categoryId, CancellationToken cancellationToken = default)
    {
        var activeCount = await _productRepository.CountActiveProductsInCategoryAsync(categoryId, cancellationToken);
        if (activeCount > 0)
        {
            throw new InvalidOperationException("Cannot delete a category that has active products.");
        }

        var category = await _categoryRepository.GetByIdAsync(categoryId, cancellationToken)
            ?? throw new InvalidOperationException("Category was not found.");

        await _categoryRepository.DeleteAsync(category, cancellationToken);
        await _categoryRepository.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Deleted category {CategoryName}.", category.Name);
    }

    public Task<int> CountActiveProductsAsync(Guid categoryId, CancellationToken cancellationToken = default) =>
        _productRepository.CountActiveProductsInCategoryAsync(categoryId, cancellationToken);
}
