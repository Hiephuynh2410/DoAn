using DoAn.Areas.Admin.Services;
using DoAn.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace DoAn.Areas.Admin.ApiAdminController
{
    [ApiController]
    [Route("api/[controller]")]
    public class RoleApiController : Controller
    {
        private readonly RoleServices _roleServices;
        private readonly DlctContext _dbContext;
        public RoleApiController(DlctContext dbContext, RoleServices roleServices)
        {
            _dbContext = dbContext;
            _roleServices = roleServices;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllRole()
        {
           var result = await _roleServices.GetAllRole();
            return Ok(result);
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateRole(Role createModel)
        {
            var role = await _roleServices.CreateRole(createModel);
            return Ok(role);
        }

        [HttpPut("update/{roleId}")]
        public async Task<IActionResult> UpdateRole(Role role, int roleId)
        {

            var result = await _roleServices.UpdatedRole(role, roleId);
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

        [HttpDelete("delete/{roleId}")]
        public async Task<IActionResult> DeleteCombo(int roleId)
        {
            var role = await _dbContext.Roles.FindAsync(roleId);
            if (role == null)
            {
                return NotFound();
            }

            _dbContext.Roles.Remove(role);
            await _dbContext.SaveChangesAsync();

            var deleteSuccessResponse = new
            {
                Message = "role deleted successfully"
            };

            return Ok(deleteSuccessResponse);
        }
    }
}
