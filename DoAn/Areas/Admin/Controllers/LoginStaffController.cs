//using DoAn.Models;
//using Microsoft.AspNetCore.Mvc;

//namespace DoAn.Areas.Admin.Controllers
//{
//    public class LoginStaffController : Controller
//    {
//        DlctContext db = new DlctContext();
//        [HttpGet]
//        public ActionResult DangNhap()
//        {
//            return View();
//        }
//        [HttpPost]
//        public ActionResult DangNhap(FormCollection collection, Staff nhanvien)
//        {
//            var tendangnhap = collection["UserName"];
//            var matkhau = collection["Password"];
//            var email = collection["Email"];

//            Staff nv = db.Staff.FirstOrDefault(x => x.Username == tendangnhap && x.Password == matkhau);

//            if (nv != null)
//            {
//                ViewBag.ThongBao = "Chúc mừng đăng nhập thà nh công";
//                HttpContext.Session.SetString("User", nv.);
//                HttpContext.Session.SetInt32("CHucVU", nv.CHUCVU.MaCV ?? 0);
//                HttpContext.Session.SetInt32("TaiKhoan", nv.MaNV);
//                HttpContext.Session.SetString("CHucVU1", nv.CHUCVU.TenCV);
//                HttpContext.Session.SetString("User", nv.UserName);
//                HttpContext.Session.SetInt32("Account", nv.MaCV ?? 0);
//                HttpContext.Session.SetString("FullTaiKhoan", nv.ToString());
//                HttpContext.Session.SetInt32("TaiKhoanAdmin", nv.MaNV ?? 0);
//                HttpContext.Session.SetString("Image", nv.HinhNV);


//            }
//            else if (nv == null)
//            {
//                ViewData["ErrorAccount"] = "sai mật khẩu hoặc Tên đăng nhập không tồn tại vui lòng nhập lại";
//                return this.DangNhap();
//            }
//            else
//            {
//                ViewData["ErrorPass"] = "Mật khẩu không đúng";
//                return this.DangNhap();
//            }
//            return RedirectToAction("Index", "NhanVien");
//        }

//        public ActionResult Logout()
//        {
//            Session["User"] = null;
//            return RedirectToAction("DangNhap", "LoginAdmin");
//        }
//    }
//}
