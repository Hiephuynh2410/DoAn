using DoAn.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using System.Text;

namespace DoAn.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class BlogController : Controller
    {
        DlctContext db = new DlctContext();

        private readonly HttpClient _httpClient;
        public BlogController()
        {
            _httpClient = new HttpClient();
        }

        //View List
        public async Task<IActionResult> Index()
        {
            var apiResponse = await _httpClient.GetAsync("https://localhost:7109/api/BlogApi/");
            if (apiResponse.IsSuccessStatusCode)
            {
                var responseContent = await apiResponse.Content.ReadAsStringAsync();
                var Blog = JsonConvert.DeserializeObject<List<BlogPost>>(responseContent);
                return View(Blog);
            }
            else
            {
                var BlogList = await db.BlogPosts
                    .Include(s => s.BlogCategory)
                    .Include(s => s.Staff)
                    .ToListAsync();
                return View(BlogList);
            }
        }

        //button choose image
        [HttpPost]
        public IActionResult ProcessUpload(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return Json("No file uploaded");
            }
            string fileName = Path.GetFileName(file.FileName);
            string filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", fileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                file.CopyTo(stream);
            }
            return Json("/images/" + fileName);
        }

        //register
        public IActionResult create()
        {

            if (HttpContext.Session.GetString("UserId") == null)
            {
                HttpContext.Session.SetString("ReturnUrl", Url.Action("Index", "Blog"));

                return RedirectToAction("Login", "Staff");
            }

            var blogcategory = db.BlogCategories.ToList();
            var staffs = db.Staff.ToList();
            ViewBag.blogcategory = new SelectList(blogcategory, "BlogCategoryId", "Title");
            ViewBag.staffs = new SelectList(staffs, "StaffId", "Name");

            return View();
        }
        [HttpPost]
        public async Task<IActionResult> create(BlogPost registrationModel)
        {
            var apiUrl = "https://localhost:7109/api/BlogApi/create";
           
            if (string.IsNullOrEmpty(registrationModel.Titile))
            {
                ModelState.AddModelError("Titile", "Title cannot be empty.");
            }
            if (string.IsNullOrEmpty(registrationModel.Body))
            {
                ModelState.AddModelError("Body", "Body cannot be empty.");
            }
           
            if (string.IsNullOrEmpty(Request.Form["StaffId"]))
            {
                ModelState.AddModelError("StaffId", "Staff is required.");
            }
            if (string.IsNullOrEmpty(Request.Form["BlogCategoryId"]))
            {
                ModelState.AddModelError("BlogCategoryId", "BlogCategory is required.");
            }
            if (ModelState.IsValid)
            {
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

                    dynamic errorResponse = JsonConvert.DeserializeObject(responseContent);

                    if (errorResponse != null)
                    {
                        ModelState.AddModelError("", errorResponse.Message);

                        if (errorResponse.Errors != null)
                        {
                            foreach (var error in errorResponse.Errors)
                            {
                                ModelState.AddModelError("", error.ToString());
                            }
                        }
                    }

                    var blogcategory = db.BlogCategories.ToList();
                    var staffs = db.Staff.ToList();
                    ViewBag.blogcategory = new SelectList(blogcategory, "BlogCategoryId", "Title");
                    ViewBag.staffs = new SelectList(staffs, "StaffId", "Name");


                    return View(registrationModel);
                }
            }
            else
            {
                var blogcategory = db.BlogCategories.ToList();
                var staffs = db.Staff.ToList();
                ViewBag.blogcategory = new SelectList(blogcategory, "BlogCategoryId", "Title");
                ViewBag.staffs = new SelectList(staffs, "StaffId", "Name");

                return View(registrationModel);
            }
        }

        //edit
        [HttpGet]
        public IActionResult Edit(int blogPostId)
        {
            if (HttpContext.Session.GetString("UserId") == null)
            {
                HttpContext.Session.SetString("ReturnUrl", Url.Action("Index", "Blog"));

                return RedirectToAction("Login", "Staff");
            }
            var Blog = db.BlogPosts
              .Include(s => s.BlogCategory)
              .Include(s => s.Staff)
              .FirstOrDefault(s => s.BlogPostId == blogPostId);

            if (Blog == null)
            {
                return NotFound();
            }

            var blogcategory = db.BlogCategories.ToList();
            var staffs = db.Staff.ToList();

            ViewBag.blogcategory = new SelectList(blogcategory, "BlogCategoryId", "Title");
            ViewBag.staffs = new SelectList(staffs, "StaffId", "Name");

            return View(Blog);
        }
        [HttpPost]
        public async Task<IActionResult> Edit(int blogPostId, BlogPost updateModel)
        {
            try
            {
                if (string.IsNullOrEmpty(updateModel.Titile))
                {
                    ModelState.AddModelError("Titile", "Title cannot be empty.");
                }
                if (string.IsNullOrEmpty(updateModel.Body))
                {
                    ModelState.AddModelError("Body", "Body cannot be empty.");
                }
                if (string.IsNullOrEmpty(updateModel.Thumbnail))
                {
                    ModelState.AddModelError("Thumbnail", "Thumbnail cannot be empty.");
                }
                if (string.IsNullOrEmpty(Request.Form["StaffId"]))
                {
                    ModelState.AddModelError("StaffId", "Staff is required.");
                }
                if (string.IsNullOrEmpty(Request.Form["BlogCategoryId"]))
                {
                    ModelState.AddModelError("BlogCategoryId", "BlogCategory is required.");
                }
                if (!ModelState.IsValid)
                {
                    var blogcategory = db.BlogCategories.ToList();
                    var staffs = db.Staff.ToList();

                    ViewBag.blogcategory = new SelectList(blogcategory, "BlogCategoryId", "Title");
                    ViewBag.staffs = new SelectList(staffs, "StaffId", "Name");
                    return View(updateModel);
                }

                var apiUrl = $"https://localhost:7109/api/BlogApi/update/{blogPostId}";

                var json = JsonConvert.SerializeObject(updateModel);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync(apiUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    try
                    {
                        var updatedBlog = await db.BlogPosts.FirstOrDefaultAsync(s => s.BlogPostId == blogPostId);
                        if (updatedBlog != null)
                        {
                            db.Entry(updatedBlog).State = EntityState.Modified;
                            await db.SaveChangesAsync();
                        }
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("", "An error occurred while saving the editor's name: " + ex.Message);
                    }
                    return RedirectToAction("Index");
                }
                else
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine("API Response Content: " + responseContent);

                    var errorResponse = JsonConvert.DeserializeObject<object>(responseContent);

                    var blogcategory = db.BlogCategories.ToList();
                    var staffs = db.Staff.ToList();

                    ViewBag.blogcategory = new SelectList(blogcategory, "BlogCategoryId", "Title");
                    ViewBag.staffs = new SelectList(staffs, "StaffId", "Name");

                    ModelState.AddModelError("", errorResponse.ToString());
                    return View(updateModel);
                }
            }

            catch (Exception ex)
            {
                // Log the exception for debugging purposes
                Console.WriteLine("An error occurred during the edit operation: " + ex.Message);

                // Add a generic error message to the ModelState
                ModelState.AddModelError("", "An error occurred during the edit operation.");
                return View(updateModel);
            }
        }

        //Delete
        public async Task<IActionResult> Delete(int blogPostId)
        {
            if (HttpContext.Session.GetString("UserId") == null)
            {
                HttpContext.Session.SetString("ReturnUrl", Url.Action("Index", "Blog"));

                return RedirectToAction("Login", "Staff");
            }
            var apiUrl = $"https://localhost:7109/api/BlogApi/delete/{blogPostId}";

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
