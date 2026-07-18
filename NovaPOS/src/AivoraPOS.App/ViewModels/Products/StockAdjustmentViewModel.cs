using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AivoraPOS.Core.Enums;
using AivoraPOS.Core.Interfaces.Security;
using AivoraPOS.Core.Interfaces.Services;
using AivoraPOS.Core.Models.Products;

namespace AivoraPOS.App.ViewModels.Products;

public partial class StockAdjustmentViewModel : ObservableObject
{
    private readonly IInventoryService _inventoryService;
    private readonly ICurrentUserService _currentUserService;
    private readonly Action<bool> _close;

    public StockAdjustmentViewModel(
        ProductVm product,
        IInventoryService inventoryService,
        ICurrentUserService currentUserService,
        Action<bool> close)
    {
        _inventoryService = inventoryService;
        _currentUserService = currentUserService;
        _close = close;

        ProductName = product.Name;
        ProductId = product.Id;
        CurrentStock = product.StockQuantity;
        NewQuantity = product.StockQuantity;
        AdjustmentDelta = 0;
        SelectedReason = StockAdjustmentReason.Restock;
    }

    public Guid ProductId { get; }
    public string ProductName { get; }
    public int CurrentStock { get; }

    public IReadOnlyList<StockAdjustmentReason> Reasons { get; } = Enum.GetValues<StockAdjustmentReason>();

    [ObservableProperty]
    private StockAdjustmentReason _selectedReason;

    [ObservableProperty]
    private int _newQuantity;

    [ObservableProperty]
    private int _adjustmentDelta;

    [ObservableProperty]
    private string? _notes;

    [ObservableProperty]
    private string? _validationMessage;

    [ObservableProperty]
    private bool _useAdjustmentMode;

    partial void OnNewQuantityChanged(int value)
    {
        AdjustmentDelta = value - CurrentStock;
    }

    partial void OnAdjustmentDeltaChanged(int value)
    {
        if (UseAdjustmentMode)
        {
            NewQuantity = Math.Max(0, CurrentStock + value);
        }
    }

    partial void OnUseAdjustmentModeChanged(bool value)
    {
        if (value)
        {
            AdjustmentDelta = NewQuantity - CurrentStock;
        }
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        ValidationMessage = null;

        if (NewQuantity < 0)
        {
            ValidationMessage = "Stock quantity cannot be negative.";
            return;
        }

        if (SelectedReason != StockAdjustmentReason.Restock && string.IsNullOrWhiteSpace(Notes))
        {
            ValidationMessage = "Notes are required for this adjustment reason.";
            return;
        }

        var userId = _currentUserService.CurrentUser?.Id;
        if (userId is null)
        {
            MessageBox.Show("You must be signed in to adjust stock.", "Stock Adjustment", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            await _inventoryService.AdjustStockAsync(new StockAdjustmentRequest
            {
                ProductId = ProductId,
                Reason = SelectedReason,
                NewQuantity = NewQuantity,
                Notes = Notes,
                UserId = userId.Value
            });

            _close(true);
        }
        catch (Exception ex)
        {
            ValidationMessage = ex.Message;
        }
    }

    [RelayCommand]
    private void Cancel() => _close(false);
}
