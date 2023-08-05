using System;
using System.Collections.Generic;

namespace DoAn.Models;

public partial class Servicetype
{
    public int ServiceTypeId { get; set; }

    public string? Name { get; set; }

    public virtual ICollection<Service> Services { get; set; } = new List<Service>();
}
