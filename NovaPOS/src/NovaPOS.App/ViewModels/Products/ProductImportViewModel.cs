using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using NovaPOS.Core.Interfaces.Security;
using NovaPOS.Core.Interfaces.Services;
using NovaPOS.Core.Models.Products;

namespace NovaPOS.App.ViewModels.Products;

public partial class ProductImportViewModel : ObservableObject
{
    private readonly IProductImportService _importService;
    private readonly ICurrentUserService _currentUserService;
    private readonly Action<bool> _close;

    public ProductImportViewModel(
        IProductImportService importService,
        ICurrentUserService currentUserService,
        Action<bool> close)
    {
        _importService = importService;
        _currentUserService = currentUserService;
        _close = close;
    }

    public ObservableCollection<ProductImportRow> PreviewRows { get; } = [];

    [ObservableProperty]
    private string? _selectedFilePath;

    [ObservableProperty]
    private string? _statusMessage;

    [ObservableProperty]
    private string? _importSummary;

    [ObservableProperty]
    private bool _isImporting;

    [RelayCommand]
    private void DownloadTemplate()
    {
        var dialog = new SaveFileDialog
        {
            Filter = "CSV files (*.csv)|*.csv",
            FileName = "product-import-template.csv"
        };

        if (dialog.ShowDialog() != true)
        {
            return;
        }

        File.WriteAllText(dialog.FileName, _importService.GetTemplateCsv(), Encoding.UTF8);
        StatusMessage = $"Template saved to {dialog.FileName}";
    }

    [RelayCommand]
    private async Task BrowseAsync()
    {
        var dialog = new OpenFileDialog
        {
            Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*"
        };

        if (dialog.ShowDialog() != true)
        {
            return;
        }

        SelectedFilePath = dialog.FileName;
        await LoadPreviewAsync();
    }

    [RelayCommand]
    private async Task ImportAsync()
    {
        if (PreviewRows.Count == 0)
        {
            StatusMessage = "Load a CSV file first.";
            return;
        }

        var userId = _currentUserService.CurrentUser?.Id;
        if (userId is null)
        {
            MessageBox.Show("You must be signed in to import products.", "Import Products", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        IsImporting = true;
        StatusMessage = null;
        ImportSummary = null;

        try
        {
            var result = await _importService.ImportAsync(PreviewRows.ToList(), userId.Value);
            ImportSummary = $"Imported {result.ImportedCount} of {result.TotalRows} rows. Skipped {result.SkippedCount}.";
            if (result.Errors.Count > 0)
            {
                ImportSummary += Environment.NewLine + string.Join(Environment.NewLine, result.Errors.Take(10));
                if (result.Errors.Count > 10)
                {
                    ImportSummary += Environment.NewLine + $"...and {result.Errors.Count - 10} more.";
                }
            }

            _close(result.ImportedCount > 0);
        }
        catch (Exception ex)
        {
            StatusMessage = ex.Message;
        }
        finally
        {
            IsImporting = false;
        }
    }

    [RelayCommand]
    private void Cancel() => _close(false);

    private async Task LoadPreviewAsync()
    {
        PreviewRows.Clear();
        StatusMessage = null;
        ImportSummary = null;

        if (string.IsNullOrWhiteSpace(SelectedFilePath) || !File.Exists(SelectedFilePath))
        {
            StatusMessage = "Select a valid CSV file.";
            return;
        }

        await using var stream = File.OpenRead(SelectedFilePath);
        var rows = await _importService.ParseAndValidateAsync(stream);

        foreach (var row in rows.Take(5))
        {
            PreviewRows.Add(row);
        }

        var invalidCount = rows.Count(x => !x.IsValid);
        StatusMessage = $"Loaded {rows.Count} rows. Previewing first {PreviewRows.Count}. Validation issues: {invalidCount}.";
    }
}
