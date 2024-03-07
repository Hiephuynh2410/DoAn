using DoAn.ApiController.Services;
using DoAn.Models;
using Microsoft.AspNetCore.Mvc;

namespace DoAn.ApiController
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class BookingDateController : Controller
    {

        private readonly BookingDateServices _bookingDateServices;
       
        public BookingDateController( BookingDateServices bookingDateServices)
        {
            _bookingDateServices = bookingDateServices;
        }

        [HttpGet("{staffId}")]
        public async Task<IActionResult> GetScheduleStaffId(int staffId)
        {
            var result = await _bookingDateServices.GetStaffAndSchedule(staffId);
            return Ok(result);
        }
    }
}
