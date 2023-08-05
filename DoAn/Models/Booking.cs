using System;
using System.Collections.Generic;

namespace DoAn.Models;

public partial class Booking
{
    public int BookingId { get; set; }

    public int? CilentId { get; set; }

    public int? StaffId { get; set; }

    public string? Phone { get; set; }

    public string? CreatedAt { get; set; }

    public DateTime? DateTime { get; set; }

    public string? Note { get; set; }

    public bool? Status { get; set; }

    public int? ComboId { get; set; }

    public virtual ICollection<Bookingdetail> Bookingdetails { get; set; } = new List<Bookingdetail>();

    public virtual Cilent? Cilent { get; set; }

    public virtual Combo? Combo { get; set; }

    public virtual Staff? Staff { get; set; }
}
