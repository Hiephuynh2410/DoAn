using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DoAn.Models;

public partial class Cilent
{
    public int CilentId { get; set; }

    public string? Name { get; set; }
    [Required]
    [RegularExpression(@"^[]A-Za-z0-9!#$%&'*+/=?^_`{|}~\\,.@()<>[-]*$", ErrorMessage = "Invalid username format")]
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

    public virtual ICollection<Bill> Bills { get; set; } = new List<Bill>();

    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    public virtual ICollection<Cart> Carts { get; set; } = new List<Cart>();

    public virtual Role? Role { get; set; }
}
