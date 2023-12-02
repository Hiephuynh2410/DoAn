using System;
using System.Collections.Generic;

namespace DoAn.Models;

public partial class Transaction
{
    public int TransactionId { get; set; }

    public int? ClietnId { get; set; }

    public string? TransactionType { get; set; }

    public double? Amount { get; set; }

    public DateTime? TransactionDate { get; set; }

    public string? PaymentMethod { get; set; }

    public string? TransactionReference { get; set; }

    public double? PromotionAmount { get; set; }

    public double? ReceivedAmount { get; set; }

    public string? Note { get; set; }

    public int? Status { get; set; }

    public int? BillId { get; set; }

    public virtual Bill? Bill { get; set; }

    public virtual Client? Clietn { get; set; }
}
