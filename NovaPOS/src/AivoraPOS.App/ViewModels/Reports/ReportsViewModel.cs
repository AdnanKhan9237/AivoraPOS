using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using Microsoft.Win32;
using AivoraPOS.App.Helpers;
using AivoraPOS.Core.Attributes;
using AivoraPOS.Core.Enums;
using AivoraPOS.Core.Interfaces.Licensing;
using AivoraPOS.Core.Interfaces.Repositories;
using AivoraPOS.Core.Interfaces.Security;
using AivoraPOS.Core.Interfaces.Services;
using AivoraPOS.Core.Models.Reports;
using AivoraPOS.Core.Exceptions;
using AivoraPOS.Licensing.Extensions;

namespace AivoraPOS.App.ViewModels.Reports;

[RequiresPermission(Permission.ViewReports)]
public partial class ReportsViewModel : ObservableObject
{
    private readonly IReportService _reportService;
    private readonly IReportExportService _reportExportService;
    private readonly ILicenseService _licenseService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAppSettingRepository _appSettingRepository;

    public ReportsViewModel(
        IReportService reportService,
        IReportExportService reportExportService,
        ILicenseService licenseService,
        ICurrentUserService currentUserService,
        IAppSettingRepository appSettingRepository)
    {
        _reportService = reportService;
        _reportExportService = reportExportService;
        _licenseService = licenseService;
        _currentUserService = currentUserService;
        _appSettingRepository = appSettingRepository;

        DailyDate = DateTime.Today;
        RangeStartDate = DateTime.Today.AddDays(-7);
        RangeEndDate = DateTime.Today;
        ProductStartDate = DateTime.Today.AddDays(-30);
        ProductEndDate = DateTime.Today;
        CashierStartDate = DateTime.Today.AddDays(-30);
        CashierEndDate = DateTime.Today;

        _ = LoadDailyAsync();
    }

    public bool HasFullReports => _licenseService.CanUse(LicenseFeature.FullReports);
    public bool CanExport => _licenseService.CanUse(LicenseFeature.ExportPdfExcel);
    public bool ShowUpgradeOverlay => !HasFullReports;
    public bool CanViewCashierReport => _currentUserService.CurrentUser?.Role == UserRole.Admin;

    [ObservableProperty]
    private DateTime _dailyDate;

    [ObservableProperty]
    private DateTime _rangeStartDate;

    [ObservableProperty]
    private DateTime _rangeEndDate;

    [ObservableProperty]
    private DateTime _productStartDate;

    [ObservableProperty]
    private DateTime _productEndDate;

    [ObservableProperty]
    private DateTime _cashierStartDate;

    [ObservableProperty]
    private DateTime _cashierEndDate;

    [ObservableProperty]
    private bool _inventoryLowStockOnly;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

  // Daily summary
    [ObservableProperty] private decimal _dailyRevenue;
    [ObservableProperty] private int _dailyTransactions;
    [ObservableProperty] private decimal _dailyAverageTicket;
    [ObservableProperty] private decimal _dailyCashRevenue;
    [ObservableProperty] private decimal _dailyCardRevenue;
    [ObservableProperty] private decimal _dailyMixedRevenue;
    [ObservableProperty] private ISeries[] _hourlySalesSeries = Array.Empty<ISeries>();
    [ObservableProperty] private Axis[] _hourlyXAxes = Array.Empty<Axis>();
    [ObservableProperty] private Axis[] _hourlyYAxes = ReportChartFactory.CreateValueYAxis();
    public ObservableCollection<TopProductDto> DailyTopProducts { get; } = new();
    public ObservableCollection<CashierPerformanceDto> DailyCashierPerformance { get; } = new();

    // Date range
    [ObservableProperty] private decimal _rangeRevenue;
    [ObservableProperty] private int _rangeTransactions;
    [ObservableProperty] private int _rangeItemsSold;
    [ObservableProperty] private ISeries[] _revenueTrendSeries = Array.Empty<ISeries>();
    [ObservableProperty] private Axis[] _revenueTrendXAxes = Array.Empty<Axis>();
    [ObservableProperty] private Axis[] _revenueTrendYAxes = ReportChartFactory.CreateValueYAxis();
    [ObservableProperty] private ISeries[] _categoryPieSeries = Array.Empty<ISeries>();
    public ObservableCollection<TopProductDto> RangeBestSellers { get; } = new();

    // Inventory
    public ObservableCollection<InventoryStatusDto> InventoryItems { get; } = new();

    // Product performance
    public ObservableCollection<ProductPerformanceDto> ProductPerformanceItems { get; } = new();

    // Cashier
    public ObservableCollection<CashierReportDto> CashierReportItems { get; } = new();

    private DailySummaryDto? _lastDailySummary;
    private DateRangeReportDto? _lastDateRangeReport;

    [RelayCommand]
    private async Task LoadDailyAsync()
    {
        if (!EnsureFullReports())
        {
            return;
        }

        IsBusy = true;
        try
        {
            var report = await _reportService.GetDailySummaryAsync(DailyDate);
            _lastDailySummary = report;
            DailyRevenue = report.TotalRevenue;
            DailyTransactions = report.TotalTransactions;
            DailyAverageTicket = report.AverageTransactionValue;
            DailyCashRevenue = report.CashRevenue;
            DailyCardRevenue = report.CardRevenue;
            DailyMixedRevenue = report.MixedRevenue;

            HourlySalesSeries = ReportChartFactory.CreateColumnSeries(report.HourlySales, "Hourly Sales");
            HourlyXAxes = ReportChartFactory.CreateCategoryXAxis(report.HourlySales);

            DailyTopProducts.Clear();
            foreach (var item in report.TopProducts)
            {
                DailyTopProducts.Add(item);
            }

            DailyCashierPerformance.Clear();
            foreach (var item in report.CashierPerformance)
            {
                DailyCashierPerformance.Add(item);
            }

            StatusMessage = $"Daily summary loaded for {DailyDate:d}.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task LoadDateRangeAsync()
    {
        if (!EnsureFullReports())
        {
            return;
        }

        IsBusy = true;
        try
        {
            var report = await _reportService.GetDateRangeReportAsync(RangeStartDate, RangeEndDate);
            _lastDateRangeReport = report;
            RangeRevenue = report.TotalRevenue;
            RangeTransactions = report.TotalTransactions;
            RangeItemsSold = report.TotalItemsSold;

            RevenueTrendSeries = ReportChartFactory.CreateLineSeries(report.RevenueTrend);
            RevenueTrendXAxes = ReportChartFactory.CreateCategoryXAxis(report.RevenueTrend, labelRotation: 45);
            CategoryPieSeries = ReportChartFactory.CreatePieSeries(report.CategoryPerformance);

            RangeBestSellers.Clear();
            foreach (var item in report.BestSellingProducts)
            {
                RangeBestSellers.Add(item);
            }

            StatusMessage = $"Date range report loaded.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task LoadInventoryAsync()
    {
        if (!EnsureFullReports())
        {
            return;
        }

        IsBusy = true;
        try
        {
            var items = await _reportService.GetInventoryStatusAsync();
            InventoryItems.Clear();

            foreach (var item in items.Where(FilterInventoryItem))
            {
                InventoryItems.Add(item);
            }

            StatusMessage = $"{InventoryItems.Count} inventory items loaded.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task LoadProductPerformanceAsync()
    {
        if (!EnsureFullReports())
        {
            return;
        }

        IsBusy = true;
        try
        {
            var items = await _reportService.GetProductPerformanceAsync(ProductStartDate, ProductEndDate);
            ProductPerformanceItems.Clear();
            foreach (var item in items)
            {
                ProductPerformanceItems.Add(item);
            }

            StatusMessage = $"{items.Count} products in performance report.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task LoadCashierReportAsync()
    {
        if (!CanViewCashierReport || !EnsureFullReports())
        {
            return;
        }

        IsBusy = true;
        try
        {
            var items = await _reportService.GetCashierReportAsync(CashierStartDate, CashierEndDate);
            CashierReportItems.Clear();
            foreach (var item in items)
            {
                CashierReportItems.Add(item);
            }

            StatusMessage = $"Cashier report loaded for {items.Count} cashiers.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task ExportDailyPdfAsync() => await ExportPdfAsync(async (path, store) =>
    {
        if (_lastDailySummary is null)
        {
            await LoadDailyAsync();
        }

        if (_lastDailySummary is not null)
        {
            await _reportExportService.ExportDailySummaryPdfAsync(_lastDailySummary, store, path);
        }
    }, "Daily_Sales_Summary.pdf");

    [RelayCommand]
    private async Task ExportDateRangePdfAsync() => await ExportPdfAsync(async (path, store) =>
    {
        if (_lastDateRangeReport is null)
        {
            await LoadDateRangeAsync();
        }

        if (_lastDateRangeReport is not null)
        {
            await _reportExportService.ExportDateRangePdfAsync(_lastDateRangeReport, store, path);
        }
    }, "Date_Range_Report.pdf");

    [RelayCommand]
    private async Task ExportProductPerformancePdfAsync() => await ExportPdfAsync(async (path, store) =>
    {
        await LoadProductPerformanceAsync();
        await _reportExportService.ExportProductPerformancePdfAsync(
            ProductPerformanceItems.ToList(), ProductStartDate, ProductEndDate, store, path);
    }, "Product_Performance.pdf");

    [RelayCommand]
    private async Task ExportInventoryPdfAsync() => await ExportPdfAsync(async (path, store) =>
    {
        await LoadInventoryAsync();
        await _reportExportService.ExportInventoryPdfAsync(InventoryItems.ToList(), store, path);
    }, "Inventory_Report.pdf");

    [RelayCommand]
    private async Task ExportInventoryCsvAsync()
    {
        if (!CanExport)
        {
            MessageBox.Show("CSV export requires a Professional plan.", "Upgrade Required", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        if (!EnsureFullReports())
        {
            return;
        }

        var dialog = new SaveFileDialog { Filter = "CSV files (*.csv)|*.csv", FileName = "Inventory_Report.csv" };
        if (dialog.ShowDialog() != true)
        {
            return;
        }

        await LoadInventoryAsync();
        await _reportExportService.ExportInventoryCsvAsync(InventoryItems.ToList(), dialog.FileName);
        StatusMessage = "Inventory exported to CSV.";
    }

    [RelayCommand]
    private async Task ExportCashierPdfAsync() => await ExportPdfAsync(async (path, store) =>
    {
        await LoadCashierReportAsync();
        await _reportExportService.ExportCashierReportPdfAsync(
            CashierReportItems.ToList(), CashierStartDate, CashierEndDate, store, path);
    }, "Cashier_Report.pdf");

    public event Action? UpgradeRequested;

    [RelayCommand]
    private void RequestUpgrade() => UpgradeRequested?.Invoke();

    partial void OnInventoryLowStockOnlyChanged(bool value) => _ = LoadInventoryAsync();

    private bool FilterInventoryItem(InventoryStatusDto item) =>
        !InventoryLowStockOnly || item.Status is InventoryStockStatus.LowStock or InventoryStockStatus.OutOfStock;

    private bool EnsureFullReports()
    {
        try
        {
            _licenseService.RequireFeature(LicenseFeature.FullReports);
            return true;
        }
        catch (LicenseRestrictionException)
        {
            StatusMessage = "Upgrade to Professional to access full reports.";
            return false;
        }
    }

    private async Task ExportPdfAsync(Func<string, string, Task> exportAction, string defaultFileName)
    {
        if (!CanExport)
        {
            MessageBox.Show("PDF export requires a Professional plan.", "Upgrade Required", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        if (!EnsureFullReports())
        {
            return;
        }

        var dialog = new SaveFileDialog { Filter = "PDF files (*.pdf)|*.pdf", FileName = defaultFileName };
        if (dialog.ShowDialog() != true)
        {
            return;
        }

        var store = await GetStoreNameAsync();
        await exportAction(dialog.FileName, store);
        StatusMessage = $"Report exported to {dialog.FileName}";
    }

    private async Task<string> GetStoreNameAsync()
    {
        var setting = await _appSettingRepository.GetByKeyAsync("Store.Name");
        return string.IsNullOrWhiteSpace(setting?.Value) ? "AivoraPOS Store" : setting.Value;
    }
}
