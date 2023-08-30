using DoAn.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DoAn.Areas.Admin.ApiAdminController
{
    [ApiController]
    [Route("api/[controller]")]
    public class RoleApiController : Controller
    {
        private readonly DlctContext _dbContext;
        public RoleApiController(DlctContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllRole()
        {
            var Roles = await _dbContext.Roles
                .ToListAsync();

            var RolessWithFullInfo = Roles.Select(s => new
            {
                s.RoleId,
                s.Name,
            }).ToList();

            return Ok(RolessWithFullInfo);
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateRole(Role createModel)
        {
            if (ModelState.IsValid)
            {
                var RoleExists = await _dbContext.Combos.AnyAsync(b => b.Name == createModel.Name);
                if (RoleExists)
                {
                    return BadRequest(new { Message = "Role already exists." });
                }

                var newRole = new Role
                {
                    Name = createModel.Name,
                };

                _dbContext.Roles.Add(newRole);
                await _dbContext.SaveChangesAsync();

                var registrationSuccessResponse = new
                {
                    Message = "Role registration successful",
                    RoleId = newRole.RoleId
                };
                return Ok(registrationSuccessResponse);
            }

            var invalidDataErrorResponse = new
            {
                Message = "Invalid Role data",
                Errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList()
            };
            return BadRequest(invalidDataErrorResponse);
        }

        [HttpPut("update/{roleId}")]
        public async Task<IActionResult> UpdateRole(int roleId, Combo updateModel)
        {
            var Roles = await _dbContext.Roles.FindAsync(roleId);
            if (Roles == null)
            {
                return NotFound();
            }

            if (!string.IsNullOrWhiteSpace(updateModel.Name))
            {
                Roles.Name = updateModel.Name;
            }
           
            _dbContext.Entry(Roles).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync();

            var updateSuccessResponse = new
            {
                Message = "Roles updated successfully"
            };

            return Ok(updateSuccessResponse);
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
