using DoAn.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DoAn.Areas.Admin.Services
{
    public class ProviderServices
    {
        private readonly DlctContext _dbContext;

        public ProviderServices(DlctContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<object>> GetAllProvider()
        {
            var provider = await _dbContext.Providers.ToListAsync();
            return provider.Select(p => new
            {
                p.ProviderId,
                p.Name,
                p.Address,
                p.Phone,
                p.Email
            }).Cast<object>().ToList();
        }
        public async Task<IActionResult> CreateProvider(Provider provider)
        {
            try
            {

                _dbContext.Providers.Add(provider);
                await _dbContext.SaveChangesAsync();

                var CreateProvider = await _dbContext.Providers.FirstOrDefaultAsync(p => p.ProviderId == provider.ProviderId);

                if (CreateProvider != null)
                {
                    var result = new
                    {
                        CreateProvider.ProviderId,
                        CreateProvider.Name,
                        CreateProvider.Phone,
                        CreateProvider.Address,
                        CreateProvider.Email
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
                Console.Error.WriteLine($"Error creating provider: {ex.Message}");
                return new StatusCodeResult(500);
            }

        }
        public async Task<IActionResult> UpdateProvider(int providerId, Provider provider)
        {
            var providerUpdate = await _dbContext.Providers.FirstOrDefaultAsync(x => x.ProviderId == providerId);

            if (providerUpdate == null)
            {
                return new NotFoundObjectResult("Not found provider");
            }

            if (!string.IsNullOrEmpty(provider.Name))
            {
                providerUpdate.Name = provider.Name;
            }

            if (!string.IsNullOrEmpty(provider.Address))
            {
                providerUpdate.Address = provider.Address;
            }

            if (!string.IsNullOrEmpty(provider.Phone))
            {
                providerUpdate.Phone = provider.Phone;
            }

            if (!string.IsNullOrEmpty(provider.Email))
            {
                providerUpdate.Email = provider.Email;
            }

            _dbContext.Entry(providerUpdate).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync();

            var updateSuccessResponse = new
            {
                Message = "Provider updated successfully",

                Name = providerUpdate.Name,
                Phone = providerUpdate.Phone,
                Address = providerUpdate.Address,
                Email = providerUpdate.Email,
            };

            return new OkObjectResult(updateSuccessResponse);
        }

        public async Task<IActionResult> DeleteAllProviderAsync(int providerId)
        {
            var providerIdToDelete = await _dbContext.Providers.FindAsync(providerId);

            if (providerIdToDelete == null)
            {
                return new NotFoundObjectResult("Not found provider");
            }

            _dbContext.Providers.Remove(providerIdToDelete);
            await _dbContext.SaveChangesAsync();

            var deleteSuccessResponse = new
            {
                Message = "provider deleted successfully"
            };

            return new OkObjectResult(deleteSuccessResponse);
        }
    }
}
