using System;
using System.Collections.Generic;

namespace DoAn.Models;

public partial class Staff
{
    public int StaffId { get; set; }

    public string? Name { get; set; }

    public string? Username { get; set; }

    public string? Password { get; set; }

    public string? Phone { get; set; }

    public string? Avatar { get; set; }

    public string? Address { get; set; }

    public string? Email { get; set; }

    public bool? Status { get; set; }

    public int? RoleId { get; set; }

    public string? CreatedAt { get; set; }

    public string? UpdatedAt { get; set; }

    public string? CreatedBy { get; set; }

    public string? UpdatedBy { get; set; }

    public int? BranchId { get; set; }

    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    public virtual Branch? Branch { get; set; }

    public virtual Role? Role { get; set; }

    public virtual ICollection<Scheduledetail> Scheduledetails { get; set; } = new List<Scheduledetail>();
}
