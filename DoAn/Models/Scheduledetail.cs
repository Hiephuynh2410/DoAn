using System;
using System.Collections.Generic;

namespace DoAn.Models;

public partial class Scheduledetail
{
    public int SchehuleId { get; set; }

    public int StaffId { get; set; }

    public DateTime? Date { get; set; }

    public virtual Schedule Schehule { get; set; } = null!;

    public virtual Staff Staff { get; set; } = null!;
}
