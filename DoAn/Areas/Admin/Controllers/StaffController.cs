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

        public IActionResult Register()
        {
            var roles = _dlctContext.Roles.ToList();
            var branches = _dlctContext.Branches.ToList();
            ViewBag.Roles = new SelectList(roles, "RoleId", "Name");
            ViewBag.Branches = new SelectList(branches, "BranchId", "Address");

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(Staff registrationModel)
        {
            var apiUrl = "https://localhost:7109/api/AdminApi/register";

            var content = new StringContent(JsonConvert.SerializeObject(registrationModel), Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(apiUrl, content);

            if (response.IsSuccessStatusCode)
            {
                var roles = _dlctContext.Roles.ToList();
                var branches = _dlctContext.Branches.ToList();
                ViewBag.Roles = new SelectList(roles, "RoleId", "Name");
                ViewBag.Branches = new SelectList(branches, "BranchId", "Address");
                var result = await response.Content.ReadAsStringAsync();
                return RedirectToAction("Index");
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                dynamic errorResponse = JsonConvert.DeserializeObject(errorContent);
                string errorMessage = errorResponse.message;

                ModelState.AddModelError(string.Empty, errorMessage);
                var roles = _dlctContext.Roles.ToList();
                var branches = _dlctContext.Branches.ToList();
                ViewBag.Roles = new SelectList(roles, "RoleId", "Name");
                ViewBag.Branches = new SelectList(branches, "BranchId", "Address");
                return View(registrationModel);
            }
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

        // Search
        public async Task<IActionResult> SearchStaff(string keyword)
        {
            List<Staff> staffList;
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://localhost:7109/");
                if (string.IsNullOrWhiteSpace(keyword))
                {
                    var response = await client.GetAsync("api/AdminApi");
                    if (response.IsSuccessStatusCode)
                    {
                        var responseContent = await response.Content.ReadAsStringAsync();
                        staffList = JsonConvert.DeserializeObject<List<Staff>>(responseContent);
                    }
                    else
                    {
                        return View("Index");
                    }
                }
                else
                {
                    var response = await client.GetAsync($"api/AdminApi/search?keyword={keyword}");
                    if (response.IsSuccessStatusCode)
                    {
                        var responseContent = await response.Content.ReadAsStringAsync();
                        staffList = JsonConvert.DeserializeObject<List<Staff>>(responseContent);
                    }
                    else
                    {
                        return View("Index");
                    }
                }
            }
            return View("Index", staffList);
        }


        //button choose image
        [HttpPost]
        public IActionResult ProcessUpload(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return Json("No file uploaded");
            }

            string fileName = Path.GetFileName(file.FileName);
            string filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                file.CopyTo(stream);
            }

            return Json("/images/" + fileName);
        }

       
        public IActionResult Chat()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("Username")))
            {
                return RedirectToAction("Login", "Staff");
            }

            var username = HttpContext.Session.GetString("Username");
            var avatar = HttpContext.Session.GetString("Avatar");

            ViewBag.Username = username;
            ViewBag.Avatar = avatar;

            return View();
        }

        [HttpGet]
        public IActionResult Sendmail(int staffId)
        {
            ViewBag.StaffId = staffId;
            return View();
        }
        //admin thich thì gửi mail ko thì thôi

        [HttpPost]
        public IActionResult Sendmail(Mails model)
        {
            int staffId = int.Parse(Request.Form["staffId"]);
            var staffMember =_dlctContext.Staff.FirstOrDefault(s => s.StaffId == staffId);

            if (staffMember != null)
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("Admin", "huynhhiepvan1998@gmail.com"));
                message.Subject = model.Subject;
                message.Body = new TextPart("plain")
                {
                    Text = model.Content
                };

                using (var client = new SmtpClient())
                {
                    client.Connect("smtp.gmail.com", 587, false);
                    client.Authenticate("huynhhiepvan1998@gmail.com", "nmqt ljyf skbz xcrs");

                    message.To.Add(new MailboxAddress(staffMember.Name, staffMember.Email));

                    client.Send(message);

                    client.Disconnect(true);
                }
            }

            return View();
        }

        public async Task<IActionResult> Index()
        {
            var apiResponse = await _httpClient.GetAsync("https://localhost:7109/api/AdminApi/");

            if (apiResponse.IsSuccessStatusCode)
            {
                var responseContent = await apiResponse.Content.ReadAsStringAsync();
                var staff = JsonConvert.DeserializeObject<List<Staff>>(responseContent);
                return View(staff);
            }
            else
            {
                var staffList = await _dlctContext.Staff
                    .Include(s => s.Branch)
                    .Include(s => s.RoleId)
                    .ToListAsync();
                return View(staffList);
            }
        }

        [HttpGet]
        public IActionResult Edit(int staffId)
        {
            if (HttpContext.Session.GetString("UserId") == null)
            {
                return RedirectToAction("Login", "Staff");
            }
            var staff = _dlctContext.Staff
              .Include(s => s.Scheduledetails)
              .ThenInclude(s => s.Schedule)
              .FirstOrDefault(s => s.StaffId == staffId);

            if (staff == null)
            {
                return NotFound();
            }

            var roles = _dlctContext.Roles.ToList();
            var branches = _dlctContext.Branches.ToList();

            ViewBag.Roles = new SelectList(roles, "RoleId", "Name");
            ViewBag.Branches = new SelectList(branches, "BranchId", "Address");

            return View(staff);
        }
        [HttpPost]
        public async Task<IActionResult> Edit(int staffId, Staff updateModel)
        {
            if (!ModelState.IsValid)
            {
                var roles = _dlctContext.Roles.ToList();
                var branches = _dlctContext.Branches.ToList();

                ViewBag.Roles = new SelectList(roles, "RoleId", "Name");
                ViewBag.Branches = new SelectList(branches, "BranchId", "Address");

                return View(updateModel);
            }

            var phoneRegex = new Regex(@"^(03|05|07|08|09|01[2|6|8|9])(?!84)[0-9]{8}$");
            if (!phoneRegex.IsMatch(updateModel.Phone) || updateModel.Phone.Length > 10)
            {
                ModelState.AddModelError("Phone", "Số điện thoại không hợp lệ");
                return View(updateModel);
            }
            updateModel.Status = Request.Form["Status"] == "true";

            var apiUrl = $"https://localhost:7109/api/AdminApi/update/{staffId}";

            var json = JsonConvert.SerializeObject(updateModel);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync(apiUrl, content);

            if (response.IsSuccessStatusCode)
            {
                try
                {
                    var updatedStaff = await _dlctContext.Staff.FirstOrDefaultAsync(s => s.StaffId == staffId);
                    if (updatedStaff != null)
                    {
                        string editorName = HttpContext.Session.GetString("Name");
                        updatedStaff.UpdatedBy = editorName;

                        _dlctContext.Entry(updatedStaff).State = EntityState.Modified;
                        await _dlctContext.SaveChangesAsync();
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "An error occurred while saving the editor's name: " + ex.Message);
                }
                return RedirectToAction("Index");
            }
            else
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine("API Response Content: " + responseContent);

                var errorResponse = JsonConvert.DeserializeObject<object>(responseContent);
                var roles = _dlctContext.Roles.ToList();
                var branches = _dlctContext.Branches.ToList();

                ViewBag.Roles = new SelectList(roles, "RoleId", "Name");
                ViewBag.Branches = new SelectList(branches, "BranchId", "Address");

                ModelState.AddModelError("", errorResponse.ToString());
                return View(updateModel);
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

