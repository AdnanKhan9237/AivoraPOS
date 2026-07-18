using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace NovaPOS.App.ViewModels.Sales;

public partial class DiscountViewModel : ObservableObject
{
    public DiscountViewModel(decimal currentOrderDiscount, IReadOnlyList<CartItemVm> cartItems)
    {
        OrderDiscountAmount = currentOrderDiscount;
        foreach (var item in cartItems)
        {
            LineDiscounts.Add(new LineDiscountVm(item.ProductId, item.ProductName, item.LineDiscount));
        }
    }

    [ObservableProperty]
    private decimal _orderDiscountAmount;

    public ObservableCollection<LineDiscountVm> LineDiscounts { get; } = new();
}

public partial class LineDiscountVm : ObservableObject
{
    public LineDiscountVm(Guid productId, string productName, decimal discountAmount)
    {
        ProductId = productId;
        ProductName = productName;
        DiscountAmount = discountAmount;
    }

    public Guid ProductId { get; }
    public string ProductName { get; }

    [ObservableProperty]
    private decimal _discountAmount;
}
