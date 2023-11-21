using DoAn.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DoAn.ApiController
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class ClientUpdateApiController : Controller
    {
        private readonly DlctContext _dbContext;
        public ClientUpdateApiController(DlctContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpPut("update/{clientId}")]
        public async Task<IActionResult> clientId(int clientId, Client updateModel)
        {
            var client = await _dbContext.Clients
                .FirstOrDefaultAsync(c => c.ClientId == clientId);

            if (client == null)
            {
                return NotFound();
            }

            client.Name = updateModel.Name;
            client.Phone = updateModel.Phone;
            client.Address = updateModel.Address;
            client.UpdatedAt = DateTime.Now;

            if (updateModel.RoleId != client.RoleId)
            {
                var newRole = await _dbContext.Roles.FindAsync(updateModel.RoleId);
                if (newRole != null)
                {
                    client.Role = newRole;
                }
            }
            _dbContext.Entry(client).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync();

            var updateSuccessResponse = new
            {
                Message = "Staff updated successfully",
                Name = client.Name,
                Phone = client.Phone,
                Address = client.Address,
                Email = client.Email
            };

            return Ok(updateSuccessResponse);
        }
    }
}
