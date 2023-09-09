namespace DoAn.Models
{
    public class ScheduledetailWithRelated
    {
            public int StaffId { get; set; }
            public int ScheduleId { get; set; }
            public DateTime Date { get; set; }

            public Staff Staff { get; set; }
            public Schedule Schedule { get; set; }
    }
}
