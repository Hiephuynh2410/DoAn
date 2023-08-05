using System;
using System.Collections.Generic;

namespace DoAn.Models;

public partial class Role
{
    public int RoleId { get; set; }

    public string? Name { get; set; }

    public virtual ICollection<Cilent> Cilents { get; set; } = new List<Cilent>();

    public virtual ICollection<Staff> Staff { get; set; } = new List<Staff>();
}
