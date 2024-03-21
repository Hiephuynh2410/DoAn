using DoAn.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;

namespace DoAn.Areas.Admin.Services
{
    public class BlogServices
    {
        private readonly DlctContext _dlctContext;


        public BlogServices(DlctContext dlctContext)
        {
            _dlctContext = dlctContext;
        }
        public async Task<List<object>> GetAllBlog()
        {
            var blogpost = await _dlctContext.BlogPosts
                .Include(s => s.BlogCategory)
                .Include(s => s.Staff)
                .ToListAsync();

            return blogpost.Select(s => new
            {
                s.BlogPostId,
                s.Titile,
                s.Body,
                s.Thumbnail,
                s.DateTime,
                s.BlogCategoryId,
                s.StaffId,
                Staff = new
                {
                    s.Staff?.StaffId,
                    s.Staff?.Name
                },
                BlogCategory = new
                {
                    s.BlogCategory?.BlogCategoryId,
                    s.BlogCategory?.Title,
                    s.BlogCategory?.Description
                },
            }).Cast<object>().ToList();
        }

        public async Task<IActionResult> CreatedBlogPost(BlogPost blog)
        {
            try
            {
                if (blog.StaffId == null || blog.BlogCategoryId== null)
                {
                    return new BadRequestObjectResult("Staff and BlogCategory are required.");
                }

                _dlctContext.BlogPosts.Add(blog);
                await _dlctContext.SaveChangesAsync();

                var createdPost = await _dlctContext.BlogPosts
                    .Include(s => s.BlogCategory)
                    .Include(s => s.Staff)
                    .FirstOrDefaultAsync(p => p.BlogPostId == blog.BlogPostId);

                if (createdPost != null)
                {
                    var result = new
                    {
                        createdPost.BlogPostId,
                        createdPost.Titile,
                        createdPost.Body,
                        createdPost.Thumbnail,
                        createdPost.DateTime,
                        createdPost.BlogCategoryId,
                        createdPost.StaffId,
                        BlogCategory = new
                        {
                            id = createdPost.BlogCategory?.BlogCategoryId,
                            title = createdPost.BlogCategory?.Title,
                            desc = createdPost.BlogCategory?.Description
                        },
                        Staff = new
                        {
                            createdPost.Staff?.StaffId,
                            createdPost.Staff?.Name,
                        },
                    };

                    return new OkObjectResult(result);
                }
                else
                {
                    return new NotFoundResult();
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error creating product: {ex.Message}");
                return new StatusCodeResult(500);
            }
        }
    
        public async Task<IActionResult> UpdatedBlogPost(BlogPost blogPost, int blogPostId)
        {
            var BlogPostToUpdate = await _dlctContext.BlogPosts
                .Include(p => p.Staff)
                .Include(p => p.BlogCategory)
                .FirstOrDefaultAsync(p => p.BlogPostId == blogPostId);

            if(BlogPostToUpdate == null)
            {
                return new NotFoundObjectResult("not found Blog");
            }

            if (!string.IsNullOrWhiteSpace(blogPost.Titile))
            {
                BlogPostToUpdate.Titile = blogPost.Titile;
            }

            if (!string.IsNullOrWhiteSpace(blogPost.Body))
            {
                BlogPostToUpdate.Body = blogPost.Body;
            }

            if (blogPost.StaffId.HasValue)
            {
                var updatedstaff = await _dlctContext.Staff.FindAsync(blogPost.StaffId);
                if (updatedstaff != null)
                {
                    BlogPostToUpdate.Staff = updatedstaff;
                }
            }

            if (blogPost.BlogCategoryId != blogPost.BlogCategoryId)
            {
                var newBlogCategory = await _dlctContext.BlogCategories.FindAsync(blogPost.BlogCategoryId);
                if (newBlogCategory != null)
                {
                    BlogPostToUpdate.BlogCategory = newBlogCategory;
                }
            }
            _dlctContext.Entry(BlogPostToUpdate).State = EntityState.Modified;
            await _dlctContext.SaveChangesAsync();

            var updateSuccessResponse = new
            {
                Message = "blogpost updated successfully"
            };

            return new OkObjectResult(updateSuccessResponse);
        }

        public async Task<IActionResult> GetBlogByCategory(int blogCategoryId)
        {
            var blogposts = await _dlctContext.BlogPosts
                .Include(s => s.BlogCategory)
                .Include(s => s.Staff)
                .Where(s => s.BlogCategoryId == blogCategoryId)
                .ToListAsync();

            if (blogposts == null || blogposts.Count == 0)
            {
                return new NotFoundObjectResult("No blog posts found for the specified category.");
            }

            var blogsByCategory = blogposts.Select(s => new
            {
                s.BlogPostId,
                s.Titile,
                s.Body,
                s.Thumbnail,
                s.DateTime,
                s.BlogCategoryId,
                s.StaffId,
                Staff = new
                {
                    s.Staff.StaffId,
                    s.Staff.Name
                },
                BlogCategory = new
                {
                    s.BlogCategory.BlogCategoryId,
                    s.BlogCategory.Title,
                    s.BlogCategory.Description
                },
            }).ToList();

            return new OkObjectResult(blogsByCategory);
        }

        public async Task<IActionResult> GetBlogPostByID(int blogPostID)
        {
            var blogpost = await _dlctContext.BlogPosts
                .Include(s => s.BlogCategory)
                .Include(s => s.Staff)
                .FirstOrDefaultAsync(s => s.BlogPostId == blogPostID);

            if (blogpost == null)
            {
                return new NotFoundObjectResult("No blog posts found for the specified category.");

            }

            var blogDetails = new
            {
                blogpost.BlogPostId,
                blogpost.Titile,
                blogpost.Body,
                blogpost.Thumbnail,
                blogpost.DateTime,
                blogpost.BlogCategoryId,
                blogpost.StaffId,
                Staff = new
                {
                    blogpost.Staff.StaffId,
                    blogpost.Staff.Name
                },
                BlogCategory = new
                {
                    blogpost.BlogCategory.BlogCategoryId,
                    blogpost.BlogCategory.Title,
                    blogpost.BlogCategory.Description
                }
            };

            return new OkObjectResult(blogDetails);
        }
    }
}
