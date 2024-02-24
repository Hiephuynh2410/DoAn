using DoAn.Areas.Admin.Services;
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
        private readonly ProviderServices _providerServices;

        public ProviderApiController(DlctContext dbContext, ProviderServices providerServices)
        {
            _dbContext = dbContext;
            _providerServices = providerServices;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllProviders()
        {
            var providersWithFullInfo = await _providerServices.GetAllProvider();
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
        public async Task<IActionResult> CreateProviders(Provider registrationModel)
        {

            var result = await _providerServices.CreateProvider(registrationModel);

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

        [HttpPut("update/{providerId}")]
        public async Task<IActionResult> UpdateProvidersAsync(int providerId, Provider provider)
        {

            var result = await _providerServices.UpdateProvider(providerId, provider);

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

        [HttpDelete("deleteAll")]
        public async Task<IActionResult> DeleteProviderAsync([FromBody] List<int> providerId)
        {
            try
            {
                foreach (var ProviderId in providerId)
                {
                    var result = await _providerServices.DeleteAllProviderAsync(ProviderId);
                }


                var deleteSuccessResponse = new
                {
                    Message = "Provider deleted successfully",
                };

                return new OkObjectResult(deleteSuccessResponse);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error deleting Provider: {ex.Message}");
                return new StatusCodeResult(500);
            }
        }
    }
}
