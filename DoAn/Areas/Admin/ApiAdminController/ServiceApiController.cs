using DoAn.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DoAn.Areas.Admin.ApiAdminController
{
    [ApiController]
    [Route("api/[controller]")]
    public class ServiceApiController : Controller
    {

        private readonly DlctContext _dbContext;
        public ServiceApiController(DlctContext dbContext)
        {
            _dbContext = dbContext;
        }


        [HttpGet("detail/{serviceId}")]
        public async Task<IActionResult> GetServiceDetail(int serviceId)
        {
            var service = await _dbContext.Services
                .Include(s => s.ServiceType)
                .FirstOrDefaultAsync(s => s.ServiceId == serviceId);

            if (service == null)
            {
                return NotFound();
            }

            var servicesDetail = new
            {
                service.ServiceId,
                service.Name,
                service.Price,
                service.Status,
                service.CreatedAt,
                service.UpdatedAt,
                service.CreatedBy,
                service.UpdatedBy,
                Servicetype = new
                {
                    service.ServiceType?.Name,
                },
            };

            return Json(servicesDetail);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllService()
        {
            var services = await _dbContext.Services
                .Include(s => s.ServiceType)
                .ToListAsync();

            var servicesWithFullInfo = services.Select(s => new
            {
                s.ServiceId,
                s.Name,
                s.Price,
                s.Status,
                s.CreatedAt,
                s.CreatedBy,
                s.UpdatedBy,
                s.UpdatedAt,
                Servicetype = new
                {
                    s.ServiceType.ServiceTypeId,
                    s.ServiceType.Name
                }
            }).ToList();

            return Ok(servicesWithFullInfo);
        }

        [HttpPost("create")]
        public async Task<IActionResult> createService(Service registrationModel)
        {
            if (ModelState.IsValid)
            {

                var serviceType = await _dbContext.Servicetypes.FindAsync(registrationModel.ServiceTypeId);

                var newService = new Service
                {
                    Name = registrationModel.Name,
                    Status = true,
                    Price = registrationModel.Price,
                    CreatedAt = DateTime.Now,
                    CreatedBy = registrationModel.CreatedBy,
                    ServiceType = serviceType,
                };

                _dbContext.Services.Add(newService);
                await _dbContext.SaveChangesAsync();

                _dbContext.Entry(newService).Reference(s => s.ServiceType).Load();

                var registrationSuccessResponse = new
                {
                    Message = "Registration successful",
                    ServiceId = newService.ServiceId,
                    Servicetype = new
                    {
                        Name = newService.ServiceType?.Name,
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

        [HttpDelete("delete/{serviceId}")]
        public async Task<IActionResult> Deleteservice(int serviceId)
        {
            var service = await _dbContext.Services.FindAsync(serviceId);

            if (service == null)
            {
                return NotFound();
            }

            _dbContext.Services.Remove(service);
            await _dbContext.SaveChangesAsync();

            var deleteSuccessResponse = new
            {
                Message = "Service deleted successfully"
            };

            return Ok(deleteSuccessResponse);
        }

        [HttpPut("update/{serviceId}")]
        public async Task<IActionResult> UpdateServices(int serviceId, Service updateModel)
        {
            var Services = await _dbContext.Services
                .FirstOrDefaultAsync(p => p.ServiceId == serviceId);

            if (Services == null)
            {
                return NotFound();
            }

            Services.Name = updateModel.Name;
            Services.Price = updateModel.Price;
            Services.UpdatedAt = DateTime.Now;
            Services.UpdatedBy = updateModel.UpdatedBy;
            if (updateModel.ServiceTypeId != Services.ServiceTypeId)
            {
                var newServices = await _dbContext.Servicetypes.FindAsync(updateModel.ServiceTypeId);
                if (newServices != null)
                {
                    Services.ServiceType = newServices;
                }
            }

            _dbContext.Entry(Services).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync();

            var updateSuccessResponse = new
            {
                Message = "Services updated successfully"
            };

            return Ok(updateSuccessResponse);
        }
    }
}
