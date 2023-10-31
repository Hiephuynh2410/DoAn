using DoAn.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DoAn.Areas.Admin.ApiAdminController
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProviderApiController : Controller
    {
        private readonly DlctContext _dbContext;
        public ProviderApiController(DlctContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllProviders()
        {
            var provider = await _dbContext.Providers
                .ToListAsync();

            var providersWithFullInfo = provider.Select(s => new
            {
                s.ProviderId,
                s.Name,
                s.Address,
                s.Email,
                s.Phone
            }).ToList();

            return Ok(providersWithFullInfo);
        }

        [HttpGet("search")]
        public async Task<IActionResult> searchProvider(string keyword)
        {
            var provider = await _dbContext.Providers
                .Where(p => p.Name.Contains(keyword) || p.ProviderId.ToString() == keyword).ToListAsync();
           
            var providersWithFullInfo = provider.Select(s => new
            {
                s.ProviderId,
                s.Name,
                s.Address,
                s.Email,
                s.Phone
            }).ToList();

            return Ok(providersWithFullInfo);
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateProvider(Provider createModel)
        {
            if (ModelState.IsValid)
            {
                var ProviderExists = await _dbContext.Providers.AnyAsync(b => b.Name == createModel.Name);
                if (ProviderExists)
                {
                    return BadRequest(new { Message = "Provider already exists." });
                }

                var newProvider = new Provider
                {
                    Name = createModel.Name,
                    Address = createModel.Address, 
                    Email = createModel.Email, 
                    Phone = createModel.Phone
                };

                _dbContext.Providers.Add(newProvider);
                await _dbContext.SaveChangesAsync();

                var registrationSuccessResponse = new
                {
                    Message = "Provider create successful",
                    ProviderId = newProvider.ProviderId
                };
                return Ok(registrationSuccessResponse);
            }

            var invalidDataErrorResponse = new
            {
                Message = "Invalid Provider data", 
                Errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList()
            };
            return BadRequest(invalidDataErrorResponse);
        }

        [HttpPut("update/{providerId}")]
        public async Task<IActionResult> UpdateProvider(int providerId, Provider updateModel)
        {
            var provider = await _dbContext.Providers.FindAsync(providerId);
            if (provider == null)
            {
                return NotFound();
            }

            if (!string.IsNullOrWhiteSpace(updateModel.Name))
            {
                provider.Name = updateModel.Name;
            }

            if (!string.IsNullOrWhiteSpace(updateModel.Address))
            {
                provider.Address = updateModel.Address;
            }

            if (!string.IsNullOrWhiteSpace(updateModel.Phone))
            {
                provider.Phone = updateModel.Phone;
            }

            if (!string.IsNullOrWhiteSpace(updateModel.Email))
            {
                provider.Email = updateModel.Email;
            }

            _dbContext.Entry(provider).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync();

            var updateSuccessResponse = new
            {
                Message = "Provider updated successfully"
            };

            return Ok(updateSuccessResponse);
        }

        [HttpDelete("delete/{providerId}")]
        public async Task<IActionResult> DeleteProvider(int providerId)
        {
            var provider = await _dbContext.Providers.FindAsync(providerId);
            if (provider == null)
            {
                return NotFound();
            }

            _dbContext.Providers.Remove(provider);
            await _dbContext.SaveChangesAsync();

            var deleteSuccessResponse = new
            {
                Message = "Provider deleted successfully"
            };

            return Ok(deleteSuccessResponse);
        }

        [HttpGet("detail/{providerId}")]
        public async Task<IActionResult> GetProviderDetail(int providerId)
        {
            var provider = await _dbContext.Providers.FindAsync(providerId);

            if (provider == null)
            {
                return NotFound();
            }
            var ProviderDetail = new
            {
                provider.ProviderId,
                provider.Name,
                provider.Email,
                provider.Phone,
                provider.Address
            };
            return Json(ProviderDetail);
        }
    }
}
