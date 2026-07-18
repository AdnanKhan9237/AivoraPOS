using AivoraPOS.Core.Models.Sales;

namespace AivoraPOS.Core.Interfaces.Services;

public interface ISaleService
{
    Task<CompletedSaleResult> CompleteSaleAsync(CompleteSaleRequest request, CancellationToken cancellationToken = default);
    Task<string> GenerateNextSaleNumberAsync(CancellationToken cancellationToken = default);
}
