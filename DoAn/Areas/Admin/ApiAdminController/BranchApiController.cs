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
        public BranchApiController(DlctContext dbContext)
        {
            _dbContext = dbContext;
        }
        [HttpGet]
        public async Task<IActionResult> GetAllBranch()
        {
            var Branch = await _dbContext.Branches
                .Include(s => s.Staff)
                .ToListAsync();

            var BranchsWithFullInfo = Branch.Select(s => new
            {
                s.BranchId,
                s.Address,
                s.Hotline,
            }).ToList();

            return Ok(BranchsWithFullInfo);
        }
        [HttpPost("create")]
        public async Task<IActionResult> CreateBranch(Branch createModel)
        {
            if (ModelState.IsValid)
            {
                var addressExists = await _dbContext.Branches.AnyAsync(b => b.Address == createModel.Address);
                if (addressExists)
                {
                    return BadRequest(new { Message = "Branch already exists." });
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
        [HttpPut("update/{branchId}")]
        public async Task<IActionResult> UpdateBranch(int branchId, Branch updateModel)
        {
            var Branch = await _dbContext.Branches.FindAsync(branchId);
            if (Branch == null)
            {
                return NotFound();
            }

            if (!string.IsNullOrWhiteSpace(updateModel.Address))
            {
                Branch.Address = updateModel.Address;
            }
            if (!string.IsNullOrWhiteSpace(updateModel.Hotline))
            {
                Branch.Hotline = updateModel.Hotline;
            }

            _dbContext.Entry(Branch).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync();

            var updateSuccessResponse = new
            {
                Message = "Branch updated successfully"
            };

            return Ok(updateSuccessResponse);
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
