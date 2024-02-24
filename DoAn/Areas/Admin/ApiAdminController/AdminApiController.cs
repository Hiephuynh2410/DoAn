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
        public AdminApiController(DlctContext dbContext, LoginServices loginService)
        {
            _dbContext = dbContext;
            _loginService = loginService;
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetStaffById(int id)
        {
            var result = await _loginService.GetStaffInfoById(id);
            return result;
        }


        [HttpPost("register")]
        public async Task<IActionResult> RegisterStaffAsync(Staff registrationModel)
        {
            var result = await _loginService.RegisterClient(registrationModel);

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
