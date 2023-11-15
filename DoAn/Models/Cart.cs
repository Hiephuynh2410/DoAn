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
        get { return Quantity * Product.Price; }
        private set { }

    }
    public void UpdateTotalAmount()
    {
        TotalAmount = Quantity * Product.Price;
    }
}
