using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Diagnostics;
using System.IO;
using NovaPOS.Core.Interfaces.Services;
using NovaPOS.Core.Models.Sales;

namespace NovaPOS.App.ViewModels.Sales;

public partial class SaleCompleteViewModel : ObservableObject
{
    private readonly CompletedSaleResult _result;
    private readonly IReceiptService _receiptService;

    public SaleCompleteViewModel(CompletedSaleResult result, bool canEmailReceipt, IReceiptService receiptService)
    {
        _result = result;
        _receiptService = receiptService;
        SaleNumber = result.Sale.SaleNumber;
        TotalAmount = result.Sale.TotalAmount;
        CanEmailReceipt = canEmailReceipt;
    }

    public string SaleNumber { get; }
    public decimal TotalAmount { get; }
    public bool CanEmailReceipt { get; }

    [RelayCommand]
    private async Task PrintReceiptAsync()
    {
        await _receiptService.PrintAsync(_result.ReceiptPdfPath);
    }

    [RelayCommand]
    private void OpenReceiptFolder()
    {
        if (File.Exists(_result.ReceiptPdfPath))
        {
            Process.Start("explorer.exe", $"/select,\"{_result.ReceiptPdfPath}\"");
        }
    }

    [RelayCommand]
    private void EmailReceipt()
    {
        if (!CanEmailReceipt)
        {
            return;
        }

        var mailto = $"mailto:?subject=NovaPOS Receipt {_result.Sale.SaleNumber}&body=Your receipt is attached at {_result.ReceiptPdfPath}";
        Process.Start(new ProcessStartInfo(mailto) { UseShellExecute = true });
    }

    [RelayCommand]
    private void Close() => OnClose?.Invoke();

    public event Action? OnClose;
}
