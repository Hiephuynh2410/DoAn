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

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? CreatedBy { get; set; }

    public int? UpdatedBy { get; set; }

    public int? BranchId { get; set; }

    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    public virtual Branch? Branch { get; set; }

    public virtual ICollection<Combo> ComboCreatedByNavigations { get; set; } = new List<Combo>();

    public virtual ICollection<Combo> ComboUpdatedByNavigations { get; set; } = new List<Combo>();

    public virtual ICollection<Product> ProductCreatedByNavigations { get; set; } = new List<Product>();

    public virtual ICollection<Product> ProductUpdatedByNavigations { get; set; } = new List<Product>();

    public virtual Role? Role { get; set; }

    public virtual ICollection<Scheduledetail> Scheduledetails { get; set; } = new List<Scheduledetail>();

    public virtual ICollection<Service> ServiceCreatedByNavigations { get; set; } = new List<Service>();

    public virtual ICollection<Service> ServiceUpdatedByNavigations { get; set; } = new List<Service>();
}
