using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Mvc;
using MimeKit;

namespace DoAn.Areas.Admin.SendMail
{
    public class sendMailController : Controller
    {
        public IActionResult Index()
        {
          
                return View();
        }
    }
}
