using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Mvc;
using MimeKit;
using DoAn.Models; 

namespace DoAn.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class HomeController : Controller
    {
        private readonly DlctContext _dbContext; 

        public HomeController(DlctContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IActionResult Index()
        {
           

            return View();
        }
    }
}
