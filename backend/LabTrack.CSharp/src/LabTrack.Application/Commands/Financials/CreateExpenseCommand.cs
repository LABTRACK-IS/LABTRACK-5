using LabTrack.Application.Dtos.Users;
using System;
using System.Collections.Generic;
using LabTrack.Domain.Enums;

namespace LabTrack.Application.Commands.Financials;

public class CreateExpenseCommand
{
    public decimal Amount { get; set; }
    public string Category { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid LaboratoryId { get; set; }
    public string RecordedByUserId { get; set; } = string.Empty;
    public Guid? WorkstationId { get; set; }
    public DateTime ExpenseDateUtc { get; set; }
    public Guid Id { get; set; }
    public Guid CreatedByUserId { get; set; }
    public Guid ExpenseId { get; set; }
    public string? VendorOrSupplier { get; set; }
    public string? WorkstationName { get; set; }
    public Guid? AssetId { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? AssetName { get; set; }
    public string LaboratoryName { get; set; } = string.Empty;
    public string VoidReason { get; set; } = string.Empty;
    public string Currency { get; set; } = string.Empty;
    public string? InvoiceOrReceiptNumber { get; set; }
    public string CreatedByUserName { get; set; } = string.Empty;
}

