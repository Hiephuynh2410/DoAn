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

        public BlogCategoryApiController(DlctContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllBlogCategory()
        {
            var BlogCategorys = await _dbContext.BlogCategories
                .ToListAsync();

            var BlogCategorysWithFullInfo = BlogCategorys.Select(s => new
            {
                s.BlogCategoryId,
                s.Title,
                s.Description,
            }).ToList();

            return Ok(BlogCategorysWithFullInfo);
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateBlogCategory(BlogCategory createModel)
        {
            if (ModelState.IsValid)
            {
                var BlogCategoryExists = await _dbContext.BlogCategories.AnyAsync(b => b.BlogCategoryId == createModel.BlogCategoryId);
                if (BlogCategoryExists)
                {
                    return BadRequest(new { Message = "BlogCategory already exists." });
                }

                var newBlogCategory = new BlogCategory
                {
                    Title = createModel.Title,
                    Description = createModel.Description,
                };

                _dbContext.BlogCategories.Add(newBlogCategory);
                await _dbContext.SaveChangesAsync();

                var registrationSuccessResponse = new
                {
                    Message = "BlogCategory registration successful",
                    BlogCategoryId = newBlogCategory.BlogCategoryId
                };
                return Ok(registrationSuccessResponse);
            }

            var invalidDataErrorResponse = new
            {
                Message = "Invalid Combo data",
                Errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList()
            };
            return BadRequest(invalidDataErrorResponse);
        }

        [HttpPut("update/{BlogcategoriesId}")]
        public async Task<IActionResult> UpdateCombo(int BlogcategoriesId, BlogCategory updateModel)
        {
            var Blogcategories = await _dbContext.BlogCategories.FindAsync(BlogcategoriesId);
            if (Blogcategories == null)
            {
                return NotFound();
            }

            if (!string.IsNullOrWhiteSpace(updateModel.Title))
            {
                Blogcategories.Title = updateModel.Title;
            }
            if (!string.IsNullOrWhiteSpace(updateModel.Description))
            {
                Blogcategories.Description = updateModel.Description;
            }
            _dbContext.Entry(Blogcategories).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync();

            var updateSuccessResponse = new
            {
                Message = "Blogcategories updated successfully"
            };

            return Ok(updateSuccessResponse);
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
