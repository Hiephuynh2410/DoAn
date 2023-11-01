using DoAn.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Text;

namespace DoAn.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class BookingFromClientController : Controller
    {
        DlctContext db = new DlctContext();
        private readonly HttpClient _httpClient;

        public BookingFromClientController()
        {
            _httpClient = new HttpClient();
        }

        // View List
        public async Task<IActionResult> Index()
        {
            if (HttpContext.Session.TryGetValue("UserId", out byte[] userIdBytes))
            {
                if (int.TryParse(Encoding.UTF8.GetString(userIdBytes), out int staffId))
                {
                    var apiResponse = await _httpClient.GetAsync("https://localhost:7109/api/ClientBookingApi");
                    if (apiResponse.IsSuccessStatusCode)
                    {
                        var responseContent = await apiResponse.Content.ReadAsStringAsync();
                        var bookingFromClients = JsonConvert.DeserializeObject<List<Booking>>(responseContent);
                        
                        var staff = await db.Staff.FirstOrDefaultAsync(s => s.StaffId == staffId);
                        if (staff != null)
                        {
                            if (staff.RoleId == 1) 
                            {
                                return View(bookingFromClients);
                            }
                            else 
                            {
                                var filteredBookings = bookingFromClients.Where(b => b.StaffId == staffId).ToList();
                                return View(filteredBookings); 
                            }
                        }
                    }
                }
            }
            return View();
        }

        //delete
        public async Task<IActionResult> Delete(int bookingId)
        {
            if (HttpContext.Session.GetString("UserId") == null)
            {
                return RedirectToAction("Login", "Staff");
            }
            var apiUrl = $"https://localhost:7109/api/ClientBookingApi/delete/{bookingId}";

            var response = await _httpClient.DeleteAsync(apiUrl);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("Index");
            }
            else
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine("API Response Content: " + responseContent);

                var errorResponse = JsonConvert.DeserializeObject<object>(responseContent);

                ModelState.AddModelError("", errorResponse.ToString());
                return RedirectToAction("Index");
            }
        }
    }
}
