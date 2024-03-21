using DoAn.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Formats.Asn1;

namespace DoAn.Areas.Admin.Services
{
    public class BlogCategoryServices
    {
        private readonly DlctContext _dlctContext;
        public BlogCategoryServices(DlctContext dlctContext)
        { 
            _dlctContext = dlctContext;
        }

        public async Task<List<object>> GetAllBlogCate()
        {
            var BlogCategorys = await _dlctContext.BlogCategories
               .ToListAsync();

            return BlogCategorys.Select(s => new
            {
                s.BlogCategoryId,
                s.Title,
                s.Description,
            }).Cast<object>().ToList();
        }

        public async Task<IActionResult> CreatedBlogCate(BlogCategory blogCategory)
        {
            try
            {
                _dlctContext.BlogCategories.Add(blogCategory);
                await _dlctContext.SaveChangesAsync();

                var ceratedBlogCate = await _dlctContext.BlogCategories
                        .FirstOrDefaultAsync(p => p.BlogCategoryId == blogCategory.BlogCategoryId);
                if (ceratedBlogCate != null)
                {
                    var result = new
                    {
                        ceratedBlogCate.BlogCategoryId,
                        ceratedBlogCate.Title,
                        ceratedBlogCate.Description
                    };
                    return new OkObjectResult(result);
                }
                else
                {
                    return new NotFoundResult();
                }
            } catch (Exception ex) 
            {
                Console.Error.WriteLine($"Error creating product: {ex.Message}");
                return new StatusCodeResult(500);
            }
        }

        public async Task<IActionResult> UpdateBlogCate(int BlogId, BlogCategory blogCategory)
        {
            var blogCate = await _dlctContext.BlogCategories.FirstOrDefaultAsync(x => x.BlogCategoryId == BlogId);
            if (blogCate != null)
            {
                return new NotFoundObjectResult("not found BlogCate");
            }
            if (!string.IsNullOrWhiteSpace(blogCategory.Title))
            {
                blogCategory.Title = blogCategory.Title;
            }
            if (!string.IsNullOrWhiteSpace(blogCategory.Description))
            {
                blogCategory.Description = blogCategory.Description;
            }

            _dlctContext.Entry(blogCategory).State = EntityState.Modified;

            await _dlctContext.SaveChangesAsync();

            var updateSuccessResponse = new
            {
                Message = "blog Cate updated successfully",
                title = blogCategory.Title,
                description = blogCategory.Description,
            };

            return new OkObjectResult(updateSuccessResponse);
        }
    }
}
