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
        public ProductApiController(DlctContext dbContext)
        {
            _dbContext = dbContext;
        }
        [HttpGet]
        public async Task<IActionResult> GetAllProducts()
        {
            var products = await _dbContext.Products
                .Include(s => s.ProductType)
                .Include(s => s.Provider)
                .ToListAsync();

            var productsWithFullInfo = products.Select(s => new
            {
                s.ProductId,
                s.Name,
                s.Description,
                s.Price,
                s.Quantity,
                s.Image,
                s.ProductTypeId,
                s.ProviderId,
                s.CreatedAt,
                s.UpdatedAt,
                s.CreatedBy,
                s.UpdatedBy,
                ProductType = new
                {
                    Name = s.ProductType?.Name
                },
                Provider = new
                {
                    s.Provider?.Name,
                    s.Provider?.Address,
                    s.Provider?.Email,
                    s.Provider?.Phone
                },
            }).ToList();

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
        public async Task<IActionResult> CreateProducts(Product registrationModel)
        {
            if (ModelState.IsValid)
            {
                var productType = await _dbContext.Producttypes.FindAsync(registrationModel.ProductTypeId);
                var provider = await _dbContext.Providers.FindAsync(registrationModel.ProviderId);

                var newProduct = new Product
                {
                    Name = registrationModel.Name,
                    Description = registrationModel.Description,
                    Price = registrationModel.Price,
                    Quantity = registrationModel.Quantity,
                    Image = registrationModel.Image,
                    CreatedAt = DateTime.Now,
                    CreatedBy = registrationModel.CreatedBy,
                    ProductType = productType,
                    Provider = provider,
                };

                _dbContext.Products.Add(newProduct);
                await _dbContext.SaveChangesAsync();
                _dbContext.Entry(newProduct).Reference(s => s.ProductType).Load();
                _dbContext.Entry(newProduct).Reference(s => s.Provider).Load();

                var registrationSuccessResponse = new
                {
                    Message = "Product created successfully",
                    ProductId = newProduct.ProductId,
                    ProductType = new
                    {
                        Address = newProduct.ProductType?.Name
                    },
                    Provider = new
                    {
                        Name = newProduct.Provider?.Name,
                        Address = newProduct.Provider?.Address,
                        Phone = newProduct.Provider?.Phone,
                        Email = newProduct.Provider?.Email
                    }
                };

                return Ok(registrationSuccessResponse);
            }

            var invalidDataErrorResponse = new
            {
                Message = "Invalid create data",
                Errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList()
            };

            return BadRequest(invalidDataErrorResponse);
        }
        [HttpPut("update/{productId}")]
        public async Task<IActionResult> UpdateProducts(int productId, Product updateModel)
        {
            var productToUpdate = await _dbContext.Products
                .Include(p => p.ProductType)
                .Include(p => p.Provider)
                .FirstOrDefaultAsync(p => p.ProductId == productId);
            if (productToUpdate == null)
            {
                return NotFound();
            }

            if (!string.IsNullOrWhiteSpace(updateModel.Name))
            {
                productToUpdate.Name = updateModel.Name;
            }

            if (!string.IsNullOrWhiteSpace(updateModel.Description))
            {
                productToUpdate.Description = updateModel.Description;
            }

            if (updateModel.Price.HasValue)
            {
                productToUpdate.Price = updateModel.Price;
            }

            if (updateModel.Quantity.HasValue)
            {
                productToUpdate.Quantity = updateModel.Quantity;
            }

          

            if (updateModel.ProductTypeId.HasValue)
            {
                var updatedProductType = await _dbContext.Producttypes.FindAsync(updateModel.ProductTypeId);
                if (updatedProductType != null)
                {
                    productToUpdate.ProductType = updatedProductType;
                }
            }

            if (updateModel.ProviderId.HasValue)
            {
                var updatedProvider = await _dbContext.Providers.FindAsync(updateModel.ProviderId);
                if (updatedProvider != null)
                {
                    productToUpdate.Provider = updatedProvider;
                }
            }

            productToUpdate.UpdatedAt = DateTime.Now;
            productToUpdate.UpdatedBy = updateModel.UpdatedBy;
            _dbContext.Entry(productToUpdate).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync();

            var updateSuccessResponse = new
            {
                Message = "Product updated successfully"
            };

            return Ok(updateSuccessResponse);
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
