using DoAn.Areas.Admin.Services;
using DoAn.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace DoAn.Areas.Admin.ApiAdminController
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductApiController : Controller
    {
        private readonly DlctContext _dbContext;
        private readonly ProductServices _productServices;
        public ProductApiController(DlctContext dbContext, ProductServices productServices)
        {
            _dbContext = dbContext;
            _productServices = productServices;
        }
        [HttpGet]
        public async Task<IActionResult> GetAllProducts()
        {
            var productsWithFullInfo = await _productServices.GetAllProduct();

            return Ok(productsWithFullInfo);
        }

        [HttpGet("search")]
        public async Task<IActionResult> searchProduct(string keyword)
        {
            var products = await _dbContext.Products
                .Include(p => p.ProductType)
                .Include(p => p.Provider)
                .Where(p =>
                        p.Name.Contains(keyword) || p.ProductId.ToString() == keyword
                )
                .ToListAsync();
            if (products.Count == 0)
            {
                return NotFound("No products found with the given keyword.");
            }

            var productWithFullInfo = products.Select(p => new
            {
                p.ProductId,
                p.Name,
                p.Description,
                p.Price,
                p.Quantity,
                p.Image,
                p.ProductTypeId,
                p.ProviderId,
                p.CreatedAt,
                p.UpdatedAt,
                p.CreatedBy,
                p.UpdatedBy,
                ProductType = new
                {
                    Name = p.ProductType?.Name
                },
                Provider = new
                {
                    p.Provider?.Name,
                    p.Provider?.Address,
                    p.Provider?.Email,
                    p.Provider?.Phone
                }
            }).ToList();
            return Ok(productWithFullInfo);
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateProductsAsync(Product registrationModel)
        {
            var result = await _productServices.CreateProductAsync(registrationModel);

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

        [HttpPut("update/{productId}")]
        public async Task<IActionResult> UpdateProductsAsync(int productId, Product updateModel)
        {
            var result = await _productServices.UpdateProductAsync(productId, updateModel);

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

        [HttpDelete("delete/{productId}")]
        public async Task<IActionResult> DeletePoducts(int productId)
        {
            var product = await _dbContext.Products.FindAsync(productId);

            if (product == null)
            {
                return NotFound();
            }

            _dbContext.Products.Remove(product);
            await _dbContext.SaveChangesAsync();

            var deleteSuccessResponse = new
            {
                Message = "product deleted successfully"
            };

            return Ok(deleteSuccessResponse);
        }

        [HttpDelete("deleteAll")]
        public async Task<IActionResult> DeleteProductsAsync([FromBody] List<int> productIds)
        {
            try
            {
                foreach (var productId in productIds)
                {
                    var result = await _productServices.DeleteAllProductAsync(productId);
                }

                var deleteSuccessResponse = new
                {
                    Message = "Products deleted successfully"
                };

                return new OkObjectResult(deleteSuccessResponse);
            }
            catch (Exception ex)
            {
                // Log the exception details
                Console.Error.WriteLine($"Error deleting products: {ex.Message}");
                return new StatusCodeResult(500);
            }
        }

        [HttpGet("detail/{productId}")]
        public async Task<IActionResult> GetProductDetail(int productId)
        {
            var produt = await _dbContext.Products
                   .Include(p => p.ProductType)
                   .Include(p => p.Provider)
                   .FirstOrDefaultAsync(p => p.ProductId == productId);
            if (produt == null)
            {
                return NotFound();
            }
            var produtDetail = new
            {
                produt.ProductId,
                produt.Name,
                produt.Description,
                produt.Price,
                produt.Quantity,
                produt.Image,
                produt.ProductTypeId,
                produt.ProviderId,
                ProductType = new
                {
                    Name = produt.ProductType?.Name
                },
                Provider = new
                {
                    produt.Provider?.Name,
                    produt.Provider?.Address,
                    produt.Provider?.Email,
                    produt.Provider?.Phone
                },
            };
            return Json(produtDetail);
        }
        [HttpGet("{productId}")]
        public async Task<IActionResult> GetProductById(int productId)
        {
            var product = await _dbContext.Products
                .Include(p => p.ProductType)
                .Include(p => p.Provider)
                .FirstOrDefaultAsync(p => p.ProductId == productId);

            if (product == null)
            {
                return NotFound();
            }

            var productDetail = new
            {
                product.ProductId,
                product.Name,
                product.Description,
                product.Price,
                product.Quantity,
                product.Image,
                product.ProductTypeId,
                product.ProviderId,
                ProductType = new
                {
                    Name = product.ProductType?.Name
                },
                Provider = new
                {
                    product.Provider?.Name,
                    product.Provider?.Address,
                    product.Provider?.Email,
                    product.Provider?.Phone
                },
            };

            return Ok(productDetail);
        }

    }
}
