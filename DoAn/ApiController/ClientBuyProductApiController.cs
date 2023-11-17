//using DoAn.Models;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using System;
//using System.Linq;
//using System.Threading.Tasks;

//namespace DoAn.ApiController
//{
//    [ApiController]
//    [Route("api/v1/[controller]")]
//    public class ClientBuyProductApiController : Controller
//    {
//        private readonly DlctContext _dbContext;

//        public ClientBuyProductApiController(DlctContext dbContext)
//        {
//            _dbContext = dbContext;
//        }

//        [HttpPost("AddToCart")]
//        public async Task<IActionResult> AddToCart([FromBody] Cart request)
//        {
//            try
//            {
//                if (request == null || request.UserId <= 0 || request.ProductId <= 0 || request.Quantity <= 0)
//                {
//                    return BadRequest("Invalid request parameters.");
//                }

//                var product = await _dbContext.Products.FindAsync(request.ProductId);
//                var client = await _dbContext.Clients.FindAsync(request.UserId);

//                if (product == null || client == null)
//                {
//                    return NotFound("Product or client not found.");
//                }

//                var existingCartItem = await _dbContext.Carts
//                    .Include(c => c.Product)
//                    .Include(c => c.User)
//                    .Where(c => c.UserId == request.UserId && c.ProductId == request.ProductId)
//                    .FirstOrDefaultAsync();

//                if (existingCartItem != null)
//                {
//                    existingCartItem.Quantity += request.Quantity;
//                }
//                else
//                {
//                    var newCartItem = new Cart
//                    {
//                        UserId = request.UserId,
//                        ProductId = request.ProductId,
//                        Quantity = request.Quantity,
//                        Product = product,
//                        User = client
//                    };

//                    _dbContext.Carts.Add(newCartItem);
//                }

//                await _dbContext.SaveChangesAsync();

//                return Ok("Product added to the cart successfully.");
//            }
//            catch (Exception ex)
//            {
//                return StatusCode(500, $"Internal server error: {ex.Message}");
//            }
//        }


//        [HttpDelete("RemoveFromCart/{userId}/{productId}/{quantity}")]
//        public async Task<IActionResult> RemoveFromCart(int userId, int productId, int quantity)
//        {
//            try
//            {
//                if (userId <= 0 || productId <= 0 || quantity <= 0)
//                {
//                    return BadRequest("Invalid request parameters.");
//                }

//                var existingCartItem = await _dbContext.Carts
//                    .Include(c => c.Product)
//                    .Include(c => c.User)
//                    .Where(c => c.UserId == userId && c.ProductId == productId)
//                    .FirstOrDefaultAsync();

//                if (existingCartItem == null)
//                {
//                    return NotFound("CartItem not found.");
//                }

//                if (existingCartItem.Quantity > quantity)
//                {
//                    existingCartItem.Quantity -= quantity;
//                }
//                else
//                {
//                    _dbContext.Carts.Remove(existingCartItem);
//                }

//                await _dbContext.SaveChangesAsync();

//                return Ok("Product removed from the cart successfully.");
//            }
//            catch (Exception ex)
//            {
//                return StatusCode(500, $"Internal server error: {ex.Message}");
//            }
//        }

//        [HttpPut("UpdateCart/{userId}/{productId}/{quantity}")]
//        public async Task<IActionResult> UpdateCart(int userId, int productId, int quantity)
//        {
//            try
//            {
//                if (userId <= 0 || productId <= 0 || quantity <= 0)
//                {
//                    return BadRequest("Invalid request parameters.");
//                }

//                var existingCartItem = await _dbContext.Carts
//                    .Include(c => c.Product)
//                    .Include(c => c.User)
//                    .Where(c => c.UserId == userId && c.ProductId == productId)
//                    .FirstOrDefaultAsync();

//                if (existingCartItem == null)
//                {
//                    return NotFound("CartItem not found.");
//                }

//                if (quantity > existingCartItem.Product.Quantity)
//                {
//                    return BadRequest("Requested quantity exceeds available stock.");
//                }

//                var originalQuantity = existingCartItem.Quantity;

//                existingCartItem.Product.Quantity -= quantity;

//                existingCartItem.Quantity = quantity;


//                var updatedTotalAmount = existingCartItem.TotalAmount;
//                await _dbContext.SaveChangesAsync();

//                return Ok($"Cart updated successfully. Updated Total Amount: {updatedTotalAmount}");

//            }
//            catch (Exception ex)
//            {
//                return StatusCode(500, $"Internal server error: {ex.Message}");
//            }
//        }

//        //[HttpPut("UpdateCart/{userId}/{productId}/{quantity}")]
//        //public async Task<IActionResult> UpdateCart(int userId, int productId, int quantity)
//        //{
//        //    try
//        //    {
//        //        if (userId <= 0 || productId <= 0 || quantity <= 0)
//        //        {
//        //            return BadRequest("Invalid request parameters.");
//        //        }

//        //        var existingCartItem = await _dbContext.Carts
//        //            .Include(c => c.Product)
//        //            .Include(c => c.User)
//        //            .Where(c => c.UserId == userId && c.ProductId == productId)
//        //            .FirstOrDefaultAsync();

//        //        if (existingCartItem == null)
//        //        {
//        //            return NotFound("CartItem not found.");
//        //        }

//        //        if (quantity > existingCartItem.Product.Quantity)
//        //        {
//        //            return BadRequest("Requested quantity exceeds available stock.");
//        //        }

//        //        existingCartItem.Product.Quantity -= quantity;

//        //        existingCartItem.Quantity = quantity;

//        //        await _dbContext.SaveChangesAsync();

//        //        return Ok("Cart updated successfully.");
//        //    }
//        //    catch (Exception ex)
//        //    {
//        //        return StatusCode(500, $"Internal server error: {ex.Message}");
//        //    }
//        //}

//    }
//}
