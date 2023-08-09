using DoAn.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;

namespace DoAn.Areas.Admin.ApiAdminController
{
    [ApiController]
    [Route("api/[controller]")]
    public class BranchApiController : Controller
    {
        private readonly DlctContext _dbContext;
        public BranchApiController(DlctContext dbContext)
        {
            _dbContext = dbContext;
        }
        [HttpGet]
        public async Task<IActionResult> GetAllBranch()
        {
            var branchs = _dbContext.Branches.ToList();
            return Ok(branchs);
        }
        [HttpPost("create")]
        public async Task<IActionResult> CreateBranch(Branch createModel)
        {
            if (ModelState.IsValid)
            {
                if (string.IsNullOrWhiteSpace(createModel.Address) || string.IsNullOrWhiteSpace(createModel.Hotline))
                {
                    var nameErrorRespone = new
                    {
                        Message = "Address and hotline cannot be empty"
                    };
                    return BadRequest(nameErrorRespone);
                }

                var newBranch = new Branch
                {
                    Address = createModel.Address,
                    Hotline = createModel.Hotline
                };

                _dbContext.Branches.Add(newBranch);
                await _dbContext.SaveChangesAsync();

                var registrationSuccessResponse = new
                {
                    Message = "Branch registration successful",
                    BranchId = newBranch.BranchId
                };
                return Ok(registrationSuccessResponse);
            }

            var invalidDataErrorResponse = new
            {
                Message = "Invalid branch data",
                Errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList()
            };
            return BadRequest(invalidDataErrorResponse);
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}
