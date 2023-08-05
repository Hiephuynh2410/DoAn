using System;
using System.Collections.Generic;

namespace DoAn.Models;

public partial class Billdetail
{
    public int BillId { get; set; }

    public int ProductId { get; set; }

    public int? Quantity { get; set; }

    public double? Price { get; set; }

    public virtual Bill Bill { get; set; } = null!;

    public virtual Product Product { get; set; } = null!;
}
