using DoAn.ApiController.Services;
using DoAn.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DoAn.ApiController
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class ClientSearchProduct : Controller
    {
        private readonly DlctContext _dbContext;
        private readonly ClientSearchProductServices _clientSearchProductServices;

        public ClientSearchProduct(DlctContext dbContext, ClientSearchProductServices clientSearchProductServices)
        {
            _dbContext = dbContext;
            _clientSearchProductServices = clientSearchProductServices;
        }

        [HttpGet("search")]
        public async Task<ActionResult> SearchProduct(string keyword)
        {
            var searchResult = await _clientSearchProductServices.SearchProduct(keyword);

            if (searchResult == null || searchResult.Count == 0)
            {
                return NotFound("No products found for the given keyword.");
            }

            return Ok(searchResult);
        }

        [HttpGet("filterProduct")]
        public async Task<ActionResult> FilterProductsByProductType(int productTypeId)
        {
            var filterResult = await _clientSearchProductServices.FilterProductsByProductType(productTypeId);

            return filterResult;
        }
    }
}