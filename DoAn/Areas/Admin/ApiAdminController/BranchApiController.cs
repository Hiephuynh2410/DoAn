using DoAn.Areas.Admin.Services;
using DoAn.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace DoAn.Areas.Admin.ApiAdminController
{
    [ApiController]
    [Route("api/[controller]")]
    public class BranchApiController : Controller
    {
        private readonly DlctContext _dbContext;
        private readonly BranchServices _branchServices;
        public BranchApiController(DlctContext dbContext, BranchServices branchServices)
        {
            _dbContext = dbContext;
            _branchServices = branchServices;
        }
        [HttpGet]
        public async Task<IActionResult> GetAllBranch()
        {
            var branchInfo = await _branchServices.GetAllBranch();
            return Ok(branchInfo);
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateBranch(Branch createModel)
        {
            var result = await _branchServices.CreateBranch(createModel);
            if( result is OkObjectResult okResutl)
            {
                return Ok(okResutl.Value);
            } else if(result is BadRequestObjectResult badRequestObjectResult)
            {
                return Ok(badRequestObjectResult.Value);
            }
            return StatusCode(500, "Internal server Errror");
        }

        [HttpPut("update/{branchId}")]
        public async Task<IActionResult> UpdateBranchsAsync(int branchId, Branch updateModel)
        {
            var result = await _branchServices.UpdateBranch(branchId, updateModel);

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

        [HttpDelete("delete/{branchId}")]
        public async Task<IActionResult> DeleteBranch(int branchId)
        {
            var branch = await _dbContext.Branches.FindAsync(branchId);
            if (branch == null)
            {
                return NotFound();
            }

            _dbContext.Branches.Remove(branch);
            await _dbContext.SaveChangesAsync();

            var deleteSuccessResponse = new
            {
                Message = "branch deleted successfully"
            };

            return Ok(deleteSuccessResponse);
        }

        [HttpDelete("deleteAll")]
        public async Task<IActionResult> DeleteBrnachAllAsync([FromBody] List<int> branchId)
        {
            try
            {
                foreach (var BranchId in branchId)
                {
                    var result = await _branchServices.DeleteAllBrnachAsync(BranchId);
                }

                var deleteSuccessResponse = new
                {
                    Message = "Branch deleted successfully"
                };

                return new OkObjectResult(deleteSuccessResponse);
            }
            catch (Exception ex)
            {
                // Log the exception details
                Console.Error.WriteLine($"Error deleting Brnach: {ex.Message}");
                return new StatusCodeResult(500);
            }
        }


        [HttpDelete("delete/{branchId}")]
        public async Task<IActionResult> DeleteBranchAsync(int branchId)
        {
            var result = await _branchServices.DeleteBranchAsync(branchId);

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

        [HttpGet("detail/{branchId}")]
        public async Task<IActionResult> GetBranchDetail(int branchId)
        {
            var branch = await _dbContext.Branches.FindAsync(branchId);

            if (branch == null)
            {
                return NotFound();
            }
            var BranchtDetail = new
            {
                branch.BranchId,
                branch.Address,
                branch.Hotline,
            };
            return Json(BranchtDetail);
        }
        [HttpGet("search")]
        public async Task<IActionResult> searchBranch(string keyword)
        {
            var branchs = await _dbContext.Branches
                .Include(p => p.Bookings)
                .Include(p => p.Staff)
                .Where(p =>
                        p.Address.Contains(keyword) || p.BranchId.ToString() == keyword
                )
                .ToListAsync();

            var BranchsWithFullInfo = branchs.Select(s => new
            {
                s.BranchId,
                s.Address,
                s.Hotline,
            }).ToList();

            return Ok(BranchsWithFullInfo);
        }
    }
}
