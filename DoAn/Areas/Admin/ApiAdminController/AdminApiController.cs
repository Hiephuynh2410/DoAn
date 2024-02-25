using DoAn.Areas.Admin.Services;
using DoAn.Data;
using DoAn.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;

namespace DoAn.Areas.Admin.ApiAdminController
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdminApiController : Controller
    {

        private readonly DlctContext _dbContext;

        private readonly LoginServices _loginService;
        private readonly StaffServives _staffServives;
        public AdminApiController(DlctContext dbContext, LoginServices loginService, StaffServives staffServives)
        {
            _dbContext = dbContext;
            _loginService = loginService;
            _staffServives = staffServives;
        }
        [HttpPut("update/{staffId}")]
        public async Task<IActionResult> UpdateProductsAsync(int staffId, Staff updateModel)
        {
            var result = await _staffServives.UpdateStaffAsync(staffId, updateModel);

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

        [HttpGet("{id}")]
        public async Task<IActionResult> GetStaffById(int id)
        {
            var result = await _loginService.GetStaffInfoById(id);
            return result;
        }

        [HttpGet] 
        public async Task<IActionResult> GetALLStaff()
        {
            var staff = await _staffServives.GetAllStaff();
            return Ok(staff);
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterStaffAsync(Staff registrationModel)
        {
            var result = await _loginService.RegisterClient(registrationModel);

            if (result == null)
            {
                return StatusCode(500, "Internal Server Error");
            }

            if (result is OkObjectResult okResult)
            {
                return Ok(okResult.Value);
            }
            else if (result is BadRequestObjectResult badRequestResult)
            {
                return BadRequest(badRequestResult.Value);
            }

            return StatusCode(500, "Internal Server Error");
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchStaff(string keyword)
        {
            var result = await _staffServives.searchStaff(keyword);

            if (result != null)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest("Invalid search result.");
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> LoginAsync(Staff loginModel)
        {
            var result = await _loginService.Login(loginModel);

            if (result is OkObjectResult okResult)
            {
                return Ok(okResult.Value);
            }
            else if (result is BadRequestObjectResult badRequestResult)
            {
                return BadRequest(badRequestResult.Value);
            }

            return StatusCode(500, "Internal Server Error");
        }

    }

}
