using System.Diagnostics;
using DoAn.Services;
using Microsoft.AspNetCore.Mvc;
using ProGCoder_MomoAPI.Models;
using ProGCoder_MomoAPI.Models.Order;

namespace ProGCoder_MomoAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HomeController : Controller
    {

        private IMomoService _momoService;

        public HomeController(IMomoService momoService)
        {
            _momoService = momoService;
        }

        [HttpPost("createPaymentUrl")]
        public async Task<IActionResult> CreatePaymentUrl([FromBody] OrderInfoModel model)
        {
            var response = await _momoService.CreatePaymentAsync(model);
            return Ok(new { payUrl = response.PayUrl });
        }

        [HttpGet]
        public IActionResult PaymentCallBack()
        {
            var response = _momoService.PaymentExecuteAsync(HttpContext.Request.Query);
            return View(response);
        }
    }

}