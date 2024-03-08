using DoAn.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MimeKit;
using MailKit.Net.Smtp;
using DoAn.ApiController.Services;
using DoAn.Areas.Admin.Services;
using Microsoft.CodeAnalysis;

namespace DoAn.ApiController
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class BookingController : Controller
    {
        private readonly DlctContext _dbContext;
        private readonly BookingServices _bookingServices;
        public BookingController(DlctContext dbContext, BookingServices bookingServices)
        {
            _dbContext = dbContext;
            _bookingServices = bookingServices;
        }


        [HttpPost("create")]
        public async Task<IActionResult> CreateBooking([FromBody] Booking registrationModel)
        {
            var result = await _bookingServices.CreateBooking(registrationModel);

            if (result is OkObjectResult okResult)
            {
                return Ok(okResult.Value);
            }
            else if (result is BadRequestObjectResult badRequestResult)
            {
                return BadRequest(badRequestResult.Value);
            }

            return StatusCode(500, "Internal Server Error");
        }

        [HttpGet]
        public async Task<IActionResult> GetAllBooking()
        {
            var AllBookingFullInfo = await _bookingServices.GetAllBooking();

            return Ok(AllBookingFullInfo);
        }

        [HttpPut("update/{bookingId}")]
        public async Task<IActionResult> UpdateBookingClient(int bookingId, Booking updateModel)
        {
            var result = await _bookingServices.UpdateBookingClient(bookingId, updateModel);

            if (result is OkObjectResult okResult)
            {
                return Ok(okResult.Value);
            }
            else if (result is NotFoundObjectResult notFoundResult)
            {
                return NotFound(notFoundResult.Value);
            }
            else
            {
                return StatusCode(500, "Internal Server Error");
            }
        }

        [HttpGet("branches")]
        public async Task<IActionResult> GetBranches()
        {
            var BranchesFullInfo = await _bookingServices.GetBranches();

            return Ok(BranchesFullInfo);
        }

        //[HttpGet("branches")]
        //public async Task<IActionResult> GetBranches()
        //{
        //    try
        //    {
        //        var branches = await _dbContext.Branches.ToListAsync();

        //        var branchData = branches.Select(branch => new
        //        {
        //            BranchId = branch.BranchId,
        //            Address = branch.Address,
        //            Hotline = branch.Hotline
        //        }).ToList();

        //        return Ok(branchData);
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, $"An error occurred: {ex.Message}");
        //    }
        //}


    }
}
