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
        public ServicesTypeApiController(DlctContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllServicesType()
        {
            var ServicesTypes = await _dbContext.Servicetypes
                .ToListAsync();

            var ServicesTypesWithFullInfo = ServicesTypes.Select(s => new
            {
                s.ServiceTypeId,
                s.Name,
               
            }).ToList();

            return Ok(ServicesTypesWithFullInfo);
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateServicesType(Servicetype createModel)
        {
            if (ModelState.IsValid)
            {
                var ServicetypeExists = await _dbContext.Servicetypes.AnyAsync(b => b.Name == createModel.Name);
                if (ServicetypeExists)
                {
                    return BadRequest(new { Message = "Servicetype already exists." });
                }

                var newServicetype = new Servicetype
                {
                    Name = createModel.Name,
                };

                _dbContext.Servicetypes.Add(newServicetype);
                await _dbContext.SaveChangesAsync();

                var registrationSuccessResponse = new
                {
                    Message = "Servicetype registration successful",
                    ServiceTypeId = newServicetype.ServiceTypeId
                };
                return Ok(registrationSuccessResponse);
            }

            var invalidDataErrorResponse = new
            {
                Message = "Invalid Servicetype data",
                Errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList()
            };
            return BadRequest(invalidDataErrorResponse);
        }

        [HttpPut("update/{Servicetypeid}")]
        public async Task<IActionResult> UpdateServicesType(int Servicetypeid, Combo updateModel)
        {
            var Servicetype = await _dbContext.Servicetypes.FindAsync(Servicetypeid);
            if (Servicetype == null)
            {
                return NotFound();
            }

            if (!string.IsNullOrWhiteSpace(updateModel.Name))
            {
                Servicetype.Name = updateModel.Name;
            }
            
            _dbContext.Entry(Servicetype).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync();

            var updateSuccessResponse = new
            {
                Message = "Servicetype updated successfully"
            };

            return Ok(updateSuccessResponse);
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
