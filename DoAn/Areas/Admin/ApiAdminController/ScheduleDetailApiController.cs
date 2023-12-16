using DoAn.Models;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MimeKit;
using System.Globalization;

namespace DoAn.Areas.Admin.ApiAdminController
{
    [ApiController]
    [Route("api/[controller]")]
    public class ScheduleDetailApiController : Controller
    {
        private readonly DlctContext db;
        public ScheduleDetailApiController(DlctContext dbContext)
        {
            db = dbContext;
        }
        [HttpGet]
        public async Task<IActionResult> GetAllScheduleDetails()
        {
            var Schedules = await db.Scheduledetails
                 .Include(s => s.Staff)
                 .Include(s => s.Schedule)
                .ToListAsync();

            var SchedulesWithFullInfo = Schedules.Select(s => new
            {
                s.ScheduleId,
                s.Date,
                s.StaffId,
                Staff = new
                {
                    s.Staff.StaffId,
                    s.Staff.Name
                },
                Schedule = s.Schedule != null ? new
                {
                    s.Schedule.ScheduleId,
                    s.Schedule.Time
                } : null
            }).ToList();
            return Ok(SchedulesWithFullInfo);
        }

        //[HttpPost("create")]
        //public async Task<IActionResult> CreateScheduleDetail(Scheduledetail inputModel)
        //{
        //    try
        //    {
        //        if (!ModelState.IsValid)
        //        {
        //            return BadRequest(new
        //            {
        //                Message = "Invalid Schedule Detail data",
        //                Errors = ModelState.Values
        //                    .SelectMany(v => v.Errors)
        //                    .Select(e => e.ErrorMessage)
        //                    .ToList()
        //            });
        //        }

        //        var staff = await db.Staff.FindAsync(inputModel.StaffId);
        //        var schedule = await db.Schedules.FindAsync(inputModel.ScheduleId);

        //        var createModel = new Scheduledetail
        //        {
        //            StaffId = inputModel.StaffId,
        //            ScheduleId = inputModel.ScheduleId,
        //            Date = inputModel.Date,
        //            Status = true,
        //            Staff = staff,
        //            Schedule = schedule
        //        };
        //        db.Scheduledetails.Add(createModel);
        //        await db.SaveChangesAsync();

        //        var registrationSuccessResponse = new
        //        {
        //            Message = "Schedule Detail registration successful",
        //            ScheduleDetailId = createModel.ScheduleId,
        //            Staff = new
        //            {
        //                StaffId = createModel.Staff?.StaffId,
        //            },
        //            Schedule = new
        //            {
        //                Time = createModel.Schedule?.Time,
        //            }
        //        };

        //        return Ok(registrationSuccessResponse);
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(StatusCodes.Status500InternalServerError, new
        //        {
        //            Message = "An unexpected error occurred. Please try again later."
        //        });
        //    }
        //}

        [HttpPost("create")]
        public async Task<IActionResult> CreateScheduleDetail([FromBody] Scheduledetail inputModel)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new
                    {
                        Message = "Invalid Schedule Detail data",
                        Errors = ModelState.Values
                            .SelectMany(v => v.Errors)
                            .Select(e => e.ErrorMessage)
                            .ToList()
                    });
                }

                var staff = await db.Staff.FindAsync(inputModel.StaffId);
                var schedule = await db.Schedules.FindAsync(inputModel.ScheduleId);

                var existingDetail = await db.Scheduledetails
                    .FirstOrDefaultAsync(sd =>
                        sd.StaffId == inputModel.StaffId &&
                        sd.ScheduleId == inputModel.ScheduleId &&
                        sd.Date == inputModel.Date);

                if (existingDetail != null)
                {
                    return BadRequest(new
                    {
                        Message = "A schedule detail already exists for the selected Staff, Schedule, and Date."
                    });
                }

                var createModel = new Scheduledetail
                {
                    StaffId = inputModel.StaffId,
                    ScheduleId = inputModel.ScheduleId,
                    Date = inputModel.Date,
                    Status = true,
                    Staff = staff,
                    Schedule = schedule
                };

                db.Scheduledetails.Add(createModel);
                await db.SaveChangesAsync();

                await SendEmailNotificationAsync(staff.Name, staff.Email, createModel);


                var registrationSuccessResponse = new
                {
                    Message = "Schedule Detail registration successful",
                    ScheduleDetailId = createModel.ScheduleId,
                    Staff = new
                    {
                        StaffId = createModel.Staff?.StaffId,
                    },
                    Schedule = new
                    {
                        Time = createModel.Schedule?.Time,
                    }
                };

                return Ok(registrationSuccessResponse);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    Message = "An unexpected error occurred. Please try again later."
                });
            }
        }

        private async Task SendEmailNotificationAsync(string recipientName, string recipientEmail, Scheduledetail scheduleDetail)
        {
            var message = new MimeMessage();

            message.From.Add(new MailboxAddress("Admin", "huynhhiepvan1998@gmail.com"));
            message.Subject = "Upcoming Work Schedule Notification";

            message.Body = new TextPart("html")
            {
                Text = $"<html><body>" +
                       $"<p><strong>Lịch làm việc của bạn: </strong></p>" +
                       $"<p><strong>Staff Name:</strong> {recipientName}</p>" +
                       $"<p><strong>Schedule Time:</strong> {scheduleDetail.Schedule?.Time}</p>" +
                       $"<p><strong>Date:</strong> {scheduleDetail.Date?.ToString("dd/MM/yyyy")}</p>" +
                       $"</body></html>"
            };

            try
            {
                using (var client = new SmtpClient())
                {
                    client.Connect("smtp.gmail.com", 587, false);
                    client.Authenticate("huynhhiepvan1998@gmail.com", "nmqt ljyf skbz xcrs");

                    message.To.Add(new MailboxAddress(recipientName, recipientEmail));

                    await client.SendAsync(message);

                    client.Disconnect(true);
                }
            } catch (Exception ex) { }
           
        }

        [HttpPut("update")]
        public async Task<IActionResult> UpdateScheduleDetail([FromQuery] int scheduleId, [FromQuery] int staffId, Scheduledetail updatedModel)
        {
            try
            {
                var existingScheduleDetail = await db.Scheduledetails
                    .FirstOrDefaultAsync(s => s.ScheduleId == scheduleId && s.StaffId == staffId);

                if (existingScheduleDetail == null)
                {
                    return NotFound(new
                    {
                        Message = "Schedule Detail not found."
                    });
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(new
                    {
                        Message = "Invalid Schedule Detail data",
                        Errors = ModelState.Values
                            .SelectMany(v => v.Errors)
                            .Select(e => e.ErrorMessage)
                            .ToList()
                    });
                }
                var staff = await db.Staff.FindAsync(updatedModel.StaffId);
                var schedule = await db.Schedules.FindAsync(updatedModel.ScheduleId);

                existingScheduleDetail.Date = updatedModel.Date;
                existingScheduleDetail.Status = updatedModel.Status;

                await db.SaveChangesAsync();

                await SendEmailAsync(staff.Name, staff.Email, existingScheduleDetail);

                var scheduleDetailIds = await db.Scheduledetails
                    .Select(sd => new { sd.ScheduleId, sd.StaffId })
                    .ToListAsync();

                return Ok(scheduleDetailIds);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    Message = "An unexpected error occurred. Please try again later."
                });
            }
        }
        private async Task SendEmailAsync(string recipientName, string recipientEmail, Scheduledetail scheduleDetail)
        {
            var message = new MimeMessage();

            message.From.Add(new MailboxAddress("Admin", "huynhhiepvan1998@gmail.com"));
            message.Subject = "Upcoming Work Schedule Notification";

            message.Body = new TextPart("html")
            {
                Text = $"<html><body>" +
                       $"<p><strong>Lịch làm việc của bạn đã được chỉnh sửa ! </strong></p>" +
                       $"<p><strong>Staff Name:</strong> {recipientName}</p>" +
                       $"<p><strong>Schedule Time:</strong> {scheduleDetail.Schedule?.Time}</p>" +
                       $"<p><strong>Date:</strong> {scheduleDetail.Date?.ToString("dd/MM/yyyy")}</p>" +
                       $"</body></html>"
            };

            try
            {
                using (var client = new SmtpClient())
                {
                    client.Connect("smtp.gmail.com", 587, false);
                    client.Authenticate("huynhhiepvan1998@gmail.com", "nmqt ljyf skbz xcrs");

                    message.To.Add(new MailboxAddress(recipientName, recipientEmail));

                    await client.SendAsync(message);

                    client.Disconnect(true);
                }
            }
            catch (Exception ex)
            {
            }
        }


        [HttpDelete("delete")]
        public async Task<IActionResult> DeleteScheduleDetail([FromQuery] int staffId, [FromQuery] int scheduleId)
        {
            try
            {
                var matchingScheduledetails = await db.Scheduledetails
                    .Where(s => s.StaffId == staffId && s.ScheduleId == scheduleId)
                    .ToListAsync();

                if (matchingScheduledetails == null || !matchingScheduledetails.Any())
                {
                    return NotFound(new
                    {
                        Message = "Scheduledetail not found."
                    });
                }

                db.Scheduledetails.RemoveRange(matchingScheduledetails);
                await db.SaveChangesAsync();

                return Ok(new
                {
                    Message = "Scheduledetail deleted successfully."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    Message = "An unexpected error occurred. Please try again later."
                });
            }
        }

    }
}
