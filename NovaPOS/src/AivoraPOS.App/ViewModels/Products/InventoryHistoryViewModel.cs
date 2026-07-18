using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AivoraPOS.Core.Interfaces.Services;

namespace AivoraPOS.App.ViewModels.Products;

public partial class InventoryHistoryViewModel : ObservableObject
{
    private readonly IInventoryService _inventoryService;
    private readonly Action _close;

    public InventoryHistoryViewModel(
        ProductVm product,
        IInventoryService inventoryService,
        Action close)
    {
        _inventoryService = inventoryService;
        _close = close;
        ProductName = product.Name;
        ProductSku = product.Sku;
        _ = LoadAsync(product.Id);
    }

    public string ProductName { get; }
    public string ProductSku { get; }

    public ObservableCollection<InventoryMovementRowVm> Movements { get; } = [];

    [RelayCommand]
    private void Close() => _close();

    private async Task LoadAsync(Guid productId)
    {
        var movements = await _inventoryService.GetMovementHistoryAsync(productId);
        Movements.Clear();
        foreach (var movement in movements)
        {
            Movements.Add(new InventoryMovementRowVm(movement));
        }
    }
}

public sealed class InventoryMovementRowVm
{
    public InventoryMovementRowVm(Core.Entities.InventoryMovement movement)
    {
        CreatedAt = movement.CreatedAt.ToLocalTime();
        MovementType = movement.MovementType.ToString();
        QuantityBefore = movement.QuantityBefore;
        QuantityChange = movement.QuantityChange;
        QuantityAfter = movement.QuantityAfter;
        Reference = movement.Reference ?? string.Empty;
    }

    public DateTime CreatedAt { get; }
    public string MovementType { get; }
    public int QuantityBefore { get; }
    public int QuantityChange { get; }
    public int QuantityAfter { get; }
    public string Reference { get; }
}
