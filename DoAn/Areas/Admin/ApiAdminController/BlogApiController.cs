using DoAn.Areas.Admin.Services;
using DoAn.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;

namespace DoAn.Areas.Admin.ApiAdminController
{
    [ApiController]
    [Route("api/[controller]")]
    public class BlogApiController : Controller
    {
        private readonly DlctContext _dbContext;
        private readonly BlogServices _blogServices;
        public BlogApiController(DlctContext dbContext, BlogServices blogServices)
        {
            _dbContext = dbContext;
            _blogServices = blogServices;
        }

        [HttpPost("create")]
        public async Task<IActionResult> createBlogPost(BlogPost registrationModel)
        {
            var result = await _blogServices.CreatedBlogPost(registrationModel);

            if (result is OkObjectResult okResult)
            {
                return Ok(okResult.Value);
            }
            else if (result is BadRequestObjectResult badRequestResult)
            {
                return BadRequest(badRequestResult.Value);
            }

            return StatusCode(500, "Internal Server Error");
        }

        [HttpPut("update/{blogPostId}")]
        public async Task<IActionResult> UpdateBlogPost(BlogPost blogPost, int blogPostId)
        {
            var result = await _blogServices.UpdatedBlogPost(blogPost, blogPostId);

            if (result is OkObjectResult okResult)
            {
                return Ok(okResult.Value);
            }
            else if (result is NotFoundObjectResult notFoundResult)
            {
                return NotFound(notFoundResult.Value);
            }
            else
            {
                return StatusCode(500, "Internal Server Error");
            }
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
           var blogpostsWithFullInfo = await _blogServices.GetAllBlog();

            return Ok(blogpostsWithFullInfo);
        }
     
        [HttpGet("GetBlogById/{blogPostId}")]
        public async Task<IActionResult> GetBlogPostById(int blogPostId)
        {
            var result = await _blogServices.GetBlogPostByID(blogPostId);

            if (result is OkObjectResult okResult)
            {
                return Ok(okResult.Value);
            }
            else if (result is NotFoundObjectResult notFoundResult)
            {
                return NotFound(notFoundResult.Value);
            }
            else
            {
                return StatusCode(500, "Internal Server Error");
            }
        }

        [HttpGet("byCategory/{blogCategoryId}")]
        public async Task<IActionResult> GetBlogsByCategory(int blogCategoryId)
        {

            var result = await _blogServices.GetBlogByCategory(blogCategoryId);

            if (result is OkObjectResult okResult)
            {
                return Ok(okResult.Value);
            }
            else if (result is NotFoundObjectResult notFoundResult)
            {
                return NotFound(notFoundResult.Value);
            }
            else
            {
                return StatusCode(500, "Internal Server Error");
            }
        }

    }
}
