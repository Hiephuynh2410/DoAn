using DoAn.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Utilities;

namespace DoAn.Areas.Admin.Services
{
    public class ServiceTypeServices
    {
        private readonly DlctContext _dbContext;

        public ServiceTypeServices(DlctContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<object>> GetAllServicesType()
        {
            var servicetypes = await _dbContext.Servicetypes.ToListAsync();
            return servicetypes.Select(s => new
            {
                s.ServiceTypeId,
                s.Name,
            }).Cast<object>().ToList(); 
        }

        public async Task<IActionResult> CreateServicesType(Servicetype servicetype)
        {
            try
            {
                _dbContext.Servicetypes.Add(servicetype);
                await _dbContext.SaveChangesAsync();

                var CreatedServicesType = await _dbContext.Servicetypes
                    .FirstOrDefaultAsync(p => p.ServiceTypeId == servicetype.ServiceTypeId);

                if (CreatedServicesType != null)
                {
                    var result = new
                    {
                        CreatedServicesType.ServiceTypeId,
                        CreatedServicesType.Name,
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
                Console.Error.WriteLine($"Error creating servicetype: {ex.Message}");
                return new StatusCodeResult(500);
            }
        }

        public async Task<IActionResult> UpdateServicesType(int servicesTypeId, Servicetype servicetype)
        {

            var ServicesTypeUpdate = await _dbContext.Servicetypes
                .FirstOrDefaultAsync(x => x.ServiceTypeId == servicesTypeId);

            if (ServicesTypeUpdate == null)
            {
                return new NotFoundObjectResult("Not found ServicesType");
            }

            if (!string.IsNullOrWhiteSpace(servicetype.Name))
            {
                ServicesTypeUpdate.Name = servicetype.Name;
            }

            _dbContext.Entry(ServicesTypeUpdate).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync();

            var updateSuccessResponse = new
            {
                Message = "Servcies type updated successfully",
                Name = servicetype.Name
            };

            return new OkObjectResult(updateSuccessResponse);
        }
    }
}
