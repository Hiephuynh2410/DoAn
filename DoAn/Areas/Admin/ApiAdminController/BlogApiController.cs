using DoAn.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DoAn.Areas.Admin.ApiAdminController
{
    [ApiController]
    [Route("api/[controller]")]
    public class BlogApiController : Controller
    {
        private readonly DlctContext _dbContext;
        public BlogApiController(DlctContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpPost("create")]
        public async Task<IActionResult> createBlogPost(BlogPost registrationModel)
        {
            if (ModelState.IsValid)
            {

                var Blogcategories = await _dbContext.BlogCategories.FindAsync(registrationModel.BlogCategoryId);
                var Staffs = await _dbContext.Staff.FindAsync(registrationModel.StaffId);
                var newBlogPost = new BlogPost
                {
                    Titile = registrationModel.Titile,
                    Body = registrationModel.Body,
                    Thumbnail = registrationModel.Thumbnail,
                    DateTime = DateTime.Now,
                    Staff = Staffs,
                    BlogCategory = Blogcategories,
                };

                _dbContext.BlogPosts.Add(newBlogPost);
                await _dbContext.SaveChangesAsync();

                _dbContext.Entry(newBlogPost).Reference(s => s.Staff).Load();
                _dbContext.Entry(newBlogPost).Reference(s => s.BlogCategory).Load();

                var registrationSuccessResponse = new
                {
                    Message = "Registration successful",
                    BlogPostId = newBlogPost.BlogPostId,
                    Staff = new
                    {
                        StaffId = newBlogPost.Staff?.StaffId,
                        Name = newBlogPost.Staff?.Name
                    },
                    BlogCategory = new
                    {
                        BlogCategoryId = newBlogPost.BlogCategory?.BlogCategoryId,
                        Title = newBlogPost.BlogCategory?.Title,
                        Description = newBlogPost?.BlogCategory?.Description
                    }
                };
                return Ok(registrationSuccessResponse);
            }

            var invalidDataErrorResponse = new
            {
                Message = "Invalid registration data",
                Errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList()
            };
            return BadRequest(invalidDataErrorResponse);
        }

        [HttpPut("update/{blogPostId}")]
        public async Task<IActionResult> UpdateBlogPost(int blogPostId, BlogPost updateModel)
        {
            var Blogposts = await _dbContext.BlogPosts
                .FirstOrDefaultAsync(p => p.BlogPostId == blogPostId);

            if (Blogposts == null)
            {
                return NotFound();
            }

            Blogposts.Titile = updateModel.Titile;
            Blogposts.Body = updateModel.Body;
            Blogposts.Thumbnail = updateModel.Thumbnail;
            if (updateModel.BlogCategoryId != Blogposts.BlogCategoryId)
            {
                var newBlogCategory = await _dbContext.BlogCategories.FindAsync(updateModel.BlogCategoryId);
                if (newBlogCategory != null)
                {
                    Blogposts.BlogCategory = newBlogCategory;
                }
            }

            if (updateModel.StaffId != Blogposts.StaffId)
            {
                var newStaff = await _dbContext.Staff.FindAsync(updateModel.StaffId);
                if (newStaff != null)
                {
                    Blogposts.Staff = newStaff;
                }
            }

            _dbContext.Entry(Blogposts).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync();

            var updateSuccessResponse = new
            {
                Message = "Blogposts updated successfully"
            };

            return Ok(updateSuccessResponse);
        }

        [HttpDelete("delete/{blogPostId}")]
        public async Task<IActionResult> DeleteStaff(int blogPostId)
        {
            var blogposts = await _dbContext.BlogPosts.FindAsync(blogPostId);

            if (blogposts == null)
            {
                return NotFound();
            }

            _dbContext.BlogPosts.Remove(blogposts);
            await _dbContext.SaveChangesAsync();

            var deleteSuccessResponse = new
            {
                Message = "blogpost deleted successfully"
            };

            return Ok(deleteSuccessResponse);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllBlogPost()
        {
            var blogpost = await _dbContext.BlogPosts
                .Include(s => s.BlogCategory)
                .Include(s => s.Staff)
                .ToListAsync();

            var blogpostsWithFullInfo = blogpost.Select(s => new
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

            return Ok(blogpostsWithFullInfo);
        }

    }
}
