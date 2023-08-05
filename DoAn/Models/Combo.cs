using System;
using System.Collections.Generic;

namespace DoAn.Models;

public partial class Combo
{
    public int ComboId { get; set; }

    public string? Name { get; set; }

    public double? Price { get; set; }

    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    public virtual ICollection<Combodetail> Combodetails { get; set; } = new List<Combodetail>();
}
