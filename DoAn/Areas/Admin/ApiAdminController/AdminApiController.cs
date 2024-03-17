using DoAn.Areas.Admin.Services;
using DoAn.Data;
using DoAn.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Mail;
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
        private readonly ForgotPassServices _forgotPassServices;

        public AdminApiController(DlctContext dbContext, 
            LoginServices loginService, 
            StaffServives staffServives,
            ForgotPassServices forgotPassServices)
        {
            _dbContext = dbContext;
            _loginService = loginService;
            _staffServives = staffServives;
            _forgotPassServices = forgotPassServices;
        }

        [HttpPost("forgotpassword/{staffId}")]
        public IActionResult ForgotPassword([FromBody] Staff model, int staffId)
        {
            if (ModelState.IsValid)
            {
                var enteredPassword = model.Password; // Lấy mật khẩu đã nhập từ người dùng

                var decryptedPassword = _forgotPassServices.GetOldPassword(model.Email, enteredPassword);

                if (decryptedPassword != null)
                {
                    return Ok($"Old password: {decryptedPassword}");
                }
                else
                {
                    return NotFound("User not found");
                }
            }
            else
            {
                return BadRequest(ModelState);
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


        [HttpDelete("delete/{staffId}")]
        public async Task<IActionResult> DeleteStaff(int staffId)
        {
            var staff = await _dbContext.Staff.FindAsync(staffId);

            if (staff == null)
            {
                return NotFound();
            }

            staff.IsDisabled = true;
            staff.Status = false;
            _dbContext.Entry(staff).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync();

            var deleteSuccessResponse = new
            {
                Message = "Staff disabled successfully"
            };

            return Ok(deleteSuccessResponse);
        }

        [HttpDelete("deleteAll")]
        public async Task<IActionResult> DeleteStaffAsync([FromBody] List<int> staffIds)
        {
            try
            {
                foreach (var staffId in staffIds)
                {
                    var result = await _staffServives.DeleteStaff(staffId);
                }

                var deleteSuccessResponse = new
                {
                    Message = "staff deleted successfully"
                };

                return new OkObjectResult(deleteSuccessResponse);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error deleting staff: {ex.Message}");
                return new StatusCodeResult(500);
            }
        }

        [HttpPut("add/{staffId}")]
        public async Task<IActionResult> addStaffsAsync(int staffId)
        {
            var result = await _staffServives.AddStaff(staffId);

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

        

     
    }

}
