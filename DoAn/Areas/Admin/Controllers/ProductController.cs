using DoAn.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace DoAn.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductController : Controller
    {
        DlctContext db = new DlctContext();
        private readonly HttpClient _httpClient;
        public ProductController()
        {
            _httpClient = new HttpClient();
        }

       
    }
}
