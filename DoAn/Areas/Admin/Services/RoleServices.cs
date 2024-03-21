using DoAn.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DoAn.Areas.Admin.Services
{
    public class RoleServices : Controller
    {
        private readonly DlctContext _dbContext;
        
        public RoleServices(DlctContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<object>> GetAllRole()
        {
            var role = await _dbContext.Roles.ToListAsync();

            return role.Select(s => new
            {
                s.RoleId,
                s.Name
            }).Cast<object>().ToList();
        }

        public async Task<IActionResult> CreateRole(Role role) 
        {
            try
            {
                _dbContext.Roles.Add(role);
                await _dbContext.SaveChangesAsync();

                var CreatedRole = await _dbContext.Roles
                    .FirstOrDefaultAsync(p => p.RoleId == role.RoleId);

                if (CreatedRole != null)
                {
                    var result = new
                    {
                        CreatedRole.RoleId,
                        CreatedRole.Name,
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
                Console.Error.WriteLine($"Error creating product: {ex.Message}");
                return new StatusCodeResult(500);
            }

        }
   
        public async Task<IActionResult> UpdatedRole(Role role, int roleId)
        {
            var roleTypeUpdate = await _dbContext.Roles
               .FirstOrDefaultAsync(x => x.RoleId == roleId);

            if (roleTypeUpdate == null)
            {
                return new NotFoundObjectResult("Not found role");
            }

            if (!string.IsNullOrWhiteSpace(role.Name))
            {
                roleTypeUpdate.Name = role.Name;
            }

            _dbContext.Entry(roleTypeUpdate).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync();

            var updateSuccessResponse = new
            {
                Message = "role updated successfully",
                Name = roleTypeUpdate.Name
            };

            return new OkObjectResult(updateSuccessResponse);
        }
   
    
    }
}
