using DoAn.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DoAn.Areas.Admin.ApiAdminController
{
    [ApiController]
    [Route("api/[controller]")]
    public class ComboApiController : Controller
    {
        private readonly DlctContext _dbContext;
        public ComboApiController(DlctContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllCombo()
        {
            var Combos = await _dbContext.Combos
                .ToListAsync();

            var BranchsWithFullInfo = Combos.Select(s => new
            {
                s.ComboId,
                s.Name,
                s.Price,
                s.CreatedAt,
                s.UpdatedAt,
                s.UpdatedBy,
                s.CreatedBy,
            }).ToList();

            return Ok(BranchsWithFullInfo);
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateCbombo(Combo createModel)
        {
            if (ModelState.IsValid)
            {
                var ComboExists = await _dbContext.Combos.AnyAsync(b => b.Name == createModel.Name);
                if (ComboExists)
                {
                    return BadRequest(new { Message = "Combo already exists." });
                }

                var newCombo = new Combo
                {
                    Name = createModel.Name,
                    Price = createModel.Price,
                    CreatedAt = DateTime.Now,
                    CreatedBy = createModel.CreatedBy,  
                };

                _dbContext.Combos.Add(newCombo);
                await _dbContext.SaveChangesAsync();

                var registrationSuccessResponse = new
                {
                    Message = "Combo registration successful",
                    ComboId = newCombo.ComboId
                };
                return Ok(registrationSuccessResponse);
            }

            var invalidDataErrorResponse = new
            {
                Message = "Invalid Combo data",
                Errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList()
            };
            return BadRequest(invalidDataErrorResponse);
        }

        [HttpPut("update/{comboId}")]
        public async Task<IActionResult> UpdateCombo(int comboId, Combo updateModel)
        {
            var combo = await _dbContext.Combos.FindAsync(comboId);
            if (combo == null)
            {
                return NotFound();
            }

            if (!string.IsNullOrWhiteSpace(updateModel.Name))
            {
                combo.Name = updateModel.Name;
            }
            if (!string.IsNullOrWhiteSpace(Convert.ToString(updateModel.Price)))
            {
                combo.Price = updateModel.Price;
            }
            combo.UpdatedAt = DateTime.Now;
            combo.UpdatedBy = updateModel.UpdatedBy;
            _dbContext.Entry(combo).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync();

            var updateSuccessResponse = new
            {
                Message = "Combo updated successfully"
            };

            return Ok(updateSuccessResponse);
        }

        [HttpDelete("delete/{comboId}")]
        public async Task<IActionResult> DeleteCombo(int comboId)
        {
            var combo = await _dbContext.Combos.FindAsync(comboId);
            if (combo == null)
            {
                return NotFound();
            }

            _dbContext.Combos.Remove(combo);
            await _dbContext.SaveChangesAsync();

            var deleteSuccessResponse = new
            {
                Message = "combo deleted successfully"
            };

            return Ok(deleteSuccessResponse);
        }

        [HttpGet("detail/{comboId}")]
        public async Task<IActionResult> GetComboDetail(int comboId)
        {
            var combo = await _dbContext.Combos.FindAsync(comboId);

            if (combo == null)
            {
                return NotFound();
            }
            var ComboDetail = new
            {
                combo.ComboId,
                combo.Name,
                combo.Price,
            };
            return Json(ComboDetail);
        }
    }
}
