using DoAn.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
namespace DoAn.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class LoginStaffController : Controller
    {
        DlctContext db = new DlctContext();
        [HttpGet]
        public ActionResult login(Staff nv)
        {
            TempData["UserRole"] = nv.RoleId;
            TempData["UserAvatar"] = nv.Avatar;
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Login(Staff staff)
        {
            var UserName = Request.Form["Username"].ToString();
            var Passwword = Request.Form["Password"].ToString();

            Staff nv = db.Staff.FirstOrDefault(x => x.Username == UserName);

            if (nv != null)
            {
                var passwordHasher = new PasswordHasher<Staff>();
                var passwordVerificationResult = passwordHasher.VerifyHashedPassword(nv, nv.Password, Passwword);

                if (passwordVerificationResult == PasswordVerificationResult.Success)
                {
                    HttpContext.Session.SetString("Username", nv.Username);
                    HttpContext.Session.SetString("Role", nv.RoleId.ToString());
                    //TempData["UserRole"] = nv.RoleId;
                    //TempData["UserAvatar"] = nv.Avatar;

                    return RedirectToAction("Index", "Combo");
                }
                else
                {
                    ViewData["ErrorPass"] = "Mật khẩu không đúng";
                }
            }
            else
            {
                ViewData["ErrorAccount"] = "sai mật khẩu hoặc Tên đăng nhập không tồn tại vui lòng nhập lại";
            }

            return View("Login");
        }

    }
}
