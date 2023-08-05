using System;
using System.Collections.Generic;

namespace DoAn.Models;

public partial class Bill
{
    public int BillId { get; set; }

    public string? CreatedAt { get; set; }

    public DateTime Date { get; set; }

    public int? CilentId { get; set; }

    public virtual ICollection<Billdetail> Billdetails { get; set; } = new List<Billdetail>();

    public virtual Cilent? Cilent { get; set; }
}
