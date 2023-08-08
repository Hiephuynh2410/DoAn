using DoAn.Data;
using DoAn.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;
using Microsoft.AspNetCore.Mvc;

namespace DoAn.Controllers
{
    public class Client : Controller
    {

        private readonly HttpClient _httpClient;
        DlctContext db = new DlctContext();
        public Client()
        {
            _httpClient = new HttpClient();
        }


        //View List
        public async Task<IActionResult> Index()
        {
            var apiResponse = await _httpClient.GetAsync("https://localhost:7109/api/ClientLogin/");
            if (apiResponse.IsSuccessStatusCode)
            {
                var responseContent = await apiResponse.Content.ReadAsStringAsync();
                var clients = JsonConvert.DeserializeObject<List<Cilent>>(responseContent);

                return View(clients);
            }
            else
            {
                return View();
            }
        }
        //Đăng kí
        public IActionResult Register()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Register(Cilent registrationModel)
        {
            var apiUrl = "https://localhost:7109/api/ClientLogin/Register"; 

            var json = JsonConvert.SerializeObject(registrationModel);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(apiUrl, content);

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
                return View(registrationModel);
            }
        }
        //Xóa
        public async Task<IActionResult> Delete(int clientId)
        {
            var apiUrl = $"https://localhost:7109/api/ClientLogin/delete/{clientId}";

            var response = await _httpClient.DeleteAsync(apiUrl);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("Index"); // Redirect to the list of clients after successful deletion
            }
            else
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine("API Response Content: " + responseContent);

                var errorResponse = JsonConvert.DeserializeObject<object>(responseContent);

                ModelState.AddModelError("", errorResponse.ToString());
                return RedirectToAction("Index"); // You can choose to handle the error scenario differently
            }
        }
        //edit
        public ActionResult Edit(int id)
        {
            var client = db.Cilents.FirstOrDefault(m => m.CilentId == id);
            if (client == null)
            {
                return View("_NotFound");
            } 
            return View(client);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, Cilent updatedClient)
        {
            if (ModelState.IsValid)
            {
                var client = db.Cilents.FirstOrDefault(m => m.CilentId == id);
                if (client == null)
                {
                    return View("_NotFound");
                }

                client.Name = updatedClient.Name;
                client.Username = updatedClient.Username;
                client.Phone = updatedClient.Phone;
                client.Address = updatedClient.Address;
                client.Email = updatedClient.Email;
                client.Avatar = updatedClient.Avatar;

                db.SaveChanges();
                return RedirectToAction("Index");
            }
            else
            {
                return View(updatedClient);
            }
        }
    }
}
