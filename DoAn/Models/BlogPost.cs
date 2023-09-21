using System;
using System.Collections.Generic;

namespace DoAn.Models;

public partial class BlogPost
{
    public int BlogPostId { get; set; }

    public string? Titile { get; set; }

    public string? Body { get; set; }

    public string? Thumbnail { get; set; }

    public DateTime? DateTime { get; set; }

    public int? BlogCategoryId { get; set; }

    public int? StaffId { get; set; }

    public virtual BlogCategory? BlogCategory { get; set; }

    public virtual Staff? Staff { get; set; }
}
