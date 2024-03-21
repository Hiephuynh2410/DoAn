using DoAn.Areas.Admin.Services;
using DoAn.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DoAn.Areas.Admin.ApiAdminController
{
    [ApiController]
    [Route("api/[controller]")]
    public class BlogCategoryApiController : Controller
    {

        private readonly DlctContext _dbContext;
        private readonly BlogCategoryServices _blogCategoryServices;
        public BlogCategoryApiController(DlctContext dbContext, BlogCategoryServices blogCategoryServices)
        {
            _dbContext = dbContext;
            _blogCategoryServices = blogCategoryServices;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllBlogCategory()
        {
            var blogCate  = await _blogCategoryServices.GetAllBlogCate();

            return Ok(blogCate);
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateBlogCategory(BlogCategory createModel)
        {
            var result = await _blogCategoryServices.CreatedBlogCate(createModel);

            if (result is OkObjectResult okResult)
            {
                return Ok(okResult.Value);
            }
            else if (result is BadRequestObjectResult badRequestObjectResult)
            {
                return BadRequest(badRequestObjectResult.Value);
            }
            return StatusCode(500, "Internal Server Error");
        }

        [HttpPut("update/{BlogcategoriesId}")]
        public async Task<IActionResult> UpdateCombo(int BlogcategoriesId, BlogCategory updateModel)
        {
            var result = await _blogCategoryServices.UpdateBlogCate(BlogcategoriesId, updateModel);

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

        [HttpDelete("delete/{BlogcategoriesId}")]
        public async Task<IActionResult> DeleteBlogcategories(int BlogcategoriesId)
        {
            var Blogcategories = await _dbContext.BlogCategories.FindAsync(BlogcategoriesId);
            if (Blogcategories == null)
            {
                return NotFound();
            }

            _dbContext.BlogCategories.Remove(Blogcategories);
            await _dbContext.SaveChangesAsync();

            var deleteSuccessResponse = new
            {
                Message = "Blogcategories deleted successfully"
            };

            return Ok(deleteSuccessResponse);
        }

        [HttpGet("detail/{BlogcategoriesId}")]
        public async Task<IActionResult> GetBlogcategoriesDetail(int BlogcategoriesId)
        {
            var Blogcategories = await _dbContext.BlogCategories.FindAsync(BlogcategoriesId);

            if (Blogcategories == null)
            {
                return NotFound();
            }
            var BlogcategoriesDetail = new
            {
                Blogcategories.BlogCategoryId,
                Blogcategories.Title,
                Blogcategories.Description,
            };
            return Json(BlogcategoriesDetail);
        }
    }
}
