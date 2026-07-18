using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AivoraPOS.Core.Enums;

namespace AivoraPOS.App.ViewModels.Sales;

public partial class PaymentViewModel : ObservableObject
{
    public PaymentViewModel(decimal totalDue)
    {
        TotalDue = totalDue;
        AmountReceived = totalDue;
        UpdateDerivedAmounts();
    }

    public decimal TotalDue { get; }

    [ObservableProperty]
    private PaymentMethod _paymentMethod = PaymentMethod.Cash;

    [ObservableProperty]
    private decimal _amountReceived;

    [ObservableProperty]
    private decimal _cashAmount;

    [ObservableProperty]
    private decimal _change;

    [ObservableProperty]
    private decimal _cardAmount;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    public bool IsConfirmed { get; private set; }

    public Array PaymentMethods { get; } = Enum.GetValues(typeof(PaymentMethod));

    public decimal AmountPaid => PaymentMethod switch
    {
        PaymentMethod.Cash => AmountReceived,
        PaymentMethod.Card => TotalDue,
        PaymentMethod.Mixed => CashAmount + CardAmount,
        _ => TotalDue
    };

    partial void OnPaymentMethodChanged(PaymentMethod value)
    {
        if (value == PaymentMethod.Card)
        {
            AmountReceived = TotalDue;
        }

        UpdateDerivedAmounts();
    }
    partial void OnAmountReceivedChanged(decimal value) => UpdateDerivedAmounts();
    partial void OnCashAmountChanged(decimal value) => UpdateDerivedAmounts();

    [RelayCommand]
    private void AppendDigit(string? digit)
    {
        if (string.IsNullOrEmpty(digit))
        {
            return;
        }

        if (PaymentMethod == PaymentMethod.Mixed)
        {
            CashAmount = AppendToAmount(CashAmount, digit);
        }
        else
        {
            AmountReceived = AppendToAmount(AmountReceived, digit);
        }
    }

    [RelayCommand]
    private void ClearEntry()
    {
        if (PaymentMethod == PaymentMethod.Mixed)
        {
            CashAmount = 0;
        }
        else
        {
            AmountReceived = 0;
        }

        UpdateDerivedAmounts();
    }

    [RelayCommand]
    private void Confirm()
    {
        if (PaymentMethod == PaymentMethod.Cash && AmountReceived < TotalDue)
        {
            StatusMessage = "Amount received is less than total due.";
            return;
        }

        if (PaymentMethod == PaymentMethod.Mixed && CashAmount + CardAmount < TotalDue)
        {
            StatusMessage = "Cash + card must cover the total.";
            return;
        }

        IsConfirmed = true;
        OnConfirmed?.Invoke();
    }

    [RelayCommand]
    private void Cancel() => OnCancelled?.Invoke();

    public event Action? OnConfirmed;
    public event Action? OnCancelled;

    private void UpdateDerivedAmounts()
    {
        Change = PaymentMethod switch
        {
            PaymentMethod.Cash => Math.Max(0, AmountReceived - TotalDue),
            PaymentMethod.Mixed => 0,
            _ => 0
        };

        CardAmount = PaymentMethod switch
        {
            PaymentMethod.Card => TotalDue,
            PaymentMethod.Mixed => Math.Max(0, TotalDue - CashAmount),
            _ => 0
        };

        StatusMessage = string.Empty;
        OnPropertyChanged(nameof(AmountPaid));
    }

    private static decimal AppendToAmount(decimal current, string digit)
    {
        if (digit == ".")
        {
            return current;
        }

        var text = current == 0 ? digit : $"{current:0.##}{digit}";
        return decimal.TryParse(text, out var value) ? value : current;
    }
}
