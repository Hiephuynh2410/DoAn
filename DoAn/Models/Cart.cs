using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace DoAn.Models;

public partial class Cart
{
    public int UserId { get; set; }

    public int ProductId { get; set; }

    public int? Quantity { get; set; }

    public virtual Product Product { get; set; } = null!;

    public virtual Client User { get; set; } = null!;
    [NotMapped]
    public double? TotalAmount
    {
        get
        {
            if (Quantity.HasValue && Product != null)
            {
                return Quantity.Value * Product.Price;
            }

            return null;
        }
        private set { }
    }

    public void UpdateTotalAmount()
    {
        if (Quantity.HasValue && Product != null)
        {
            TotalAmount = Quantity.Value * Product.Price;
        }
        else
        {
            TotalAmount = null;
        }
    }
}
