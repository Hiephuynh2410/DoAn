using System;
using System.Collections.Generic;

namespace DoAn.Models;

public partial class Schedule
{
    public int ScheduleId { get; set; }

    public TimeSpan? Time { get; set; }

    public virtual ICollection<Scheduledetail> Scheduledetails { get; set; } = new List<Scheduledetail>();
}
