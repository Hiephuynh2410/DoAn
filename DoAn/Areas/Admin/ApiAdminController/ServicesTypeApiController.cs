using DoAn.Areas.Admin.Services;
using DoAn.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DoAn.Areas.Admin.ApiAdminController
{
    [ApiController]
    [Route("api/[controller]")]
    public class ServicesTypeApiController : Controller
    {
        private readonly DlctContext _dbContext;
        private readonly ServiceTypeServices _serviceTypeServices;
        public ServicesTypeApiController(DlctContext dbContext, ServiceTypeServices serviceTypeServices)
        {
            _dbContext = dbContext;
            _serviceTypeServices = serviceTypeServices; 
        }

        [HttpGet]
        public async Task<IActionResult> GetAllServicesType()
        {
            var ServiceTypeInfo = await _serviceTypeServices.GetAllServicesType();
            return Ok(ServiceTypeInfo);
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchServiceType(string keyword)
        {
            var ServicesType = await _dbContext.Servicetypes
                .Where(s => s.Name.Contains(keyword) || s.ServiceTypeId.ToString() == keyword).ToListAsync();
            var servicesTypeWithFullInfo = ServicesType.Select(s => new
            {
                s.ServiceTypeId,
                s.Name,
            }).ToList();
            return Ok(servicesTypeWithFullInfo);
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateProductsType(Servicetype registrationModel)
        {

            var result = await _serviceTypeServices.CreateServicesType(registrationModel);

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

        [HttpPut("update/{serviceTypeId}")]
        public async Task<IActionResult> UpdateServicesTypesAsync(int serviceTypeId, Servicetype servicetype)
        {

            var result = await _serviceTypeServices.UpdateServicesType(serviceTypeId, servicetype);

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

        [HttpDelete("delete/{Servicetypeid}")]
        public async Task<IActionResult> DeleteServicesType(int Servicetypeid)
        {
            var Servicetype = await _dbContext.Servicetypes.FindAsync(Servicetypeid);
            if (Servicetype == null)
            {
                return NotFound();
            }

            _dbContext.Servicetypes.Remove(Servicetype);
            await _dbContext.SaveChangesAsync();

            var deleteSuccessResponse = new
            {
                Message = "Servicetype deleted successfully"
            };

            return Ok(deleteSuccessResponse);
        }

        [HttpGet("detail/{Servicetypeid}")]
        public async Task<IActionResult> GetServicetypeDetail(int Servicetypeid)
        {
            var Servicetype = await _dbContext.Servicetypes.FindAsync(Servicetypeid);

            if (Servicetype == null)
            {
                return NotFound();
            }
            var ServicetypeDetail = new
            {
                Servicetype.ServiceTypeId,
                Servicetype.Name
             
            };
            return Json(ServicetypeDetail);
        }

    }
}
