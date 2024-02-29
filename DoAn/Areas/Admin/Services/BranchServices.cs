using DoAn.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DoAn.Areas.Admin.Services
{
    public class BranchServices
    {
        private readonly DlctContext _dbContext;

        public BranchServices(DlctContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<object>> GetAllBranch()
        {
            var branches = await _dbContext.Branches.ToListAsync();

            return branches.Select(p => new
            {
                p.BranchId,
                p.Address,
                p.Hotline,
            }).Cast<object>().ToList();
        }

        public async Task<object> CreateBranch(Branch createModel)
        {
            try
            {
                _dbContext.Branches.Add(createModel);
                await _dbContext.SaveChangesAsync();

                var CreatedBranch = await _dbContext.Branches
                    .FirstOrDefaultAsync(p => p.BranchId == createModel.BranchId);

                if (CreatedBranch != null)
                {
                    var result = new
                    {
                        CreatedBranch.BranchId,
                        CreatedBranch.Address,
                        CreatedBranch.Hotline,
                    };
                    return new OkObjectResult(result);
                }
                else
                {
                    return new NotFoundResult();
                }

            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error creating product: {ex.Message}");
                return new StatusCodeResult(500);
            }
        }

        public async Task<IActionResult> UpdateBranch(int branchId, Branch branch)
        {

            var BranchUpdate = await _dbContext.Branches
                .FirstOrDefaultAsync(x => x.BranchId == branchId);

            if (BranchUpdate == null)
            {
                return new NotFoundObjectResult("Not found Branch");
            }

            if (!string.IsNullOrWhiteSpace(branch.Address))
            {
                BranchUpdate.Address= branch.Address;
            }
            if (!string.IsNullOrWhiteSpace(branch.Hotline))
            {
                BranchUpdate.Hotline = branch.Hotline;
            }

            _dbContext.Entry(BranchUpdate).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync();

            var updateSuccessResponse = new
            {
                Message = "Branch updated successfully",
                BranchId= branch.BranchId,
                Name = branch.Address,
                Hotline = branch.Hotline,
            };

            return new OkObjectResult(updateSuccessResponse);
        }

        public async Task<IActionResult> DeleteAllBrnachAsync(int branchId)
        {
            var BrnachToDelete = await _dbContext.Branches.FindAsync(branchId);

            if (BrnachToDelete == null)
            {
                return new NotFoundObjectResult("Not found Branch");
            }

            _dbContext.Branches.Remove(BrnachToDelete);
            await _dbContext.SaveChangesAsync();

            var deleteSuccessResponse = new
            {
                Message = "brnach deleted successfully"
            };

            return new OkObjectResult(deleteSuccessResponse);
        }


        public async Task<IActionResult> DeleteBranchAsync(int branchId)
        {
            try
            {
                var branch = await _dbContext.Branches.FindAsync(branchId);

                if (branch == null)
                {
                    return new NotFoundObjectResult("branch not found.");
                }

                _dbContext.Branches.Remove(branch);
                await _dbContext.SaveChangesAsync();

                var deleteSuccessResponse = new
                {
                    Message = "Bracnh deleted successfully",
                };

                return new OkObjectResult(deleteSuccessResponse);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error deleting Bracnh: {ex.Message}");
                return new StatusCodeResult(500);
            }
        }
    }
}
