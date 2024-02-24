using DoAn.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Identity;
using MimeKit;
using MailKit.Net.Smtp;
using PagedList;

using Microsoft.AspNetCore.Http;

namespace DoAn.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class StaffController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly DlctContext _dlctContext;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public StaffController(DlctContext dlctContext, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = new HttpClient();
            _dlctContext = dlctContext;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<IActionResult> Login(string username, string password)
        {
            var apiUrl = "https://localhost:7109/api/AdminApi/login";
            var loginModel = new Staff { Username = username, Password = password };
            var content = new StringContent(JsonConvert.SerializeObject(loginModel), Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(apiUrl, content);

            if (response.IsSuccessStatusCode)
            {
                var token = await response.Content.ReadAsStringAsync();

                _httpContextAccessor.HttpContext.Session.SetString("AccessToken", token);

                var staff = await _dlctContext.Staff.FirstOrDefaultAsync(c => c.Username == username);

                _httpContextAccessor.HttpContext.Session.SetString("Username", staff.Username);
                if (staff.Avatar != null)
                {
                    _httpContextAccessor.HttpContext.Session.SetString("Avatar", staff.Avatar);
                }
                _httpContextAccessor.HttpContext.Session.SetString("UserId", staff.StaffId.ToString());
                _httpContextAccessor.HttpContext.Session.SetString("Role", staff.RoleId.ToString());
                _httpContextAccessor.HttpContext.Session.SetString("Name", staff.Name);

                return RedirectToAction("Index", "Home");
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                dynamic errorResponse = JsonConvert.DeserializeObject(errorContent);
                string errorMessage = errorResponse.message;
                ModelState.AddModelError(string.Empty, errorMessage);
                return View();
            }
        }
        //logout
        public IActionResult Logout()
        {
            HttpContext.Session.Remove("Username");
            HttpContext.Session.Remove("Avatar");
            HttpContext.Session.Remove("Role");

            HttpContext.Session.Clear();

            return RedirectToAction("Index", "Home");
        }
    }
}

