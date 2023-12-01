using System.Diagnostics;
using DoAn.Services;
using Microsoft.AspNetCore.Mvc;
using ProGCoder_MomoAPI.Models.Order;

namespace DoAn.Controllers;

[Route("api/[controller]")]
[ApiController]
public class HomeController : Controller
{
    private readonly IMomoService _momoService;

    public HomeController(IMomoService momoService)
    {
        _momoService = momoService;
    }

    [HttpPost("CreatePaymentUrl")]
    public async Task<IActionResult> CreatePaymentUrl(OrderInfoModel model)
    {
        var response = await _momoService.CreatePaymentAsync(model);
        return Redirect(response.PayUrl);
    }

    [HttpGet("PaymentCallBack")]
    public IActionResult PaymentCallBack()
    {
        var response = _momoService.PaymentExecuteAsync(HttpContext.Request.Query);
        return Ok(response);
    }
}
