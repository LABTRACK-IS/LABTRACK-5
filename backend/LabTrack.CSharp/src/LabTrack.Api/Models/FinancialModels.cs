using System.ComponentModel.DataAnnotations;

namespace LabTrack.Api.Models.Contracts;

public class CreateExpenseRequest
{
    [Required]
    public string Category { get; set; } = string.Empty;

    [Required]
    public decimal Amount { get; set; }

    public string Description { get; set; } = string.Empty;
}

public class ExpenseResponse
{
    public string Id { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime TransactionDateUtc { get; set; }
}

public sealed record FinancialTransactionDto(
    Guid Id,
    Guid? AssetId,
    Guid? RepairTicketId,
    string TransactionType,
    decimal Amount,
    string Currency,
    string Description,
    DateTime TransactionDate,
    DateTime CreatedAt
);

public sealed record CreateFinancialTransactionDto(
    Guid? AssetId,
    Guid? RepairTicketId,
    string TransactionType,
    decimal Amount,
    string Currency,
    string Description,
    DateTime TransactionDate
);

public sealed record FinancialSummaryDto(
    decimal TotalExpenditure,
    decimal TotalRepairCosts,
    decimal TotalAssetAcquisitions,
    int TotalTransactionsCount
);


