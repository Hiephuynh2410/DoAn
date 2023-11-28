﻿using DoAn.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace DoAn.ApiController
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class ClientBuyProductApiController : Controller
    {
        private readonly DlctContext _dbContext;

        public ClientBuyProductApiController(DlctContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpPost("AddToCart")]
        public async Task<IActionResult> AddToCart([FromBody] Cart request)
        {
            try
            {
                if (request == null || request.UserId <= 0 || request.ProductId <= 0)
                {
                    return BadRequest("Invalid request parameters.");
                }

                var product = await _dbContext.Products.FindAsync(request.ProductId);
                var client = await _dbContext.Clients.FindAsync(request.UserId);

                if (product == null || client == null)
                {
                    return NotFound("Product or client not found.");
                }

                var quantityToAdd = request.Quantity > 0 ? request.Quantity : 1;

                if (product.Quantity < quantityToAdd)
                {
                    return BadRequest("Not enough stock available.");
                }

                var existingCartItem = await _dbContext.Carts
                    .Include(c => c.Product)
                    .Include(c => c.User)
                    .Where(c => c.UserId == request.UserId && c.ProductId == request.ProductId)
                    .FirstOrDefaultAsync();

                if (existingCartItem != null)
                {
                    var newTotalQuantity = existingCartItem.Quantity + quantityToAdd;

                    if (product.Quantity < newTotalQuantity)
                    {
                        return BadRequest("Not enough stock available for the requested quantity.");
                    }

                    existingCartItem.Quantity = newTotalQuantity;
                }
                else
                {
                    if (product.Quantity >= quantityToAdd)
                    {
                        var newCartItem = new Cart
                        {
                            UserId = request.UserId,
                            ProductId = request.ProductId,
                            Quantity = quantityToAdd,
                            Product = product,
                            User = client
                        };

                        _dbContext.Carts.Add(newCartItem);
                    }
                    else
                    {
                        return BadRequest("Product is out of stock.");
                    }
                }

                var cartItems = await _dbContext.Carts
                    .Where(c => c.UserId == request.UserId)
                    .ToListAsync();

                foreach (var cartItem in cartItems)
                {
                    cartItem.UpdateTotalAmount();
                }

                await _dbContext.SaveChangesAsync();

                var totalAmount = cartItems.Sum(c => c.TotalAmount.GetValueOrDefault());

                var responseMessage = $"Product added to the cart successfully. Total amount: {totalAmount}.";

                return Ok(responseMessage);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpDelete("RemoveFromCart/{userId}/{productId}/{quantity}")]
        public async Task<IActionResult> RemoveFromCart(int userId, int productId, int quantity)
        {
            try
            {
                if (userId <= 0 || productId <= 0 || quantity <= 0)
                {
                    return BadRequest("Invalid request parameters.");
                }

                var existingCartItem = await _dbContext.Carts
                    .Include(c => c.Product)
                    .Include(c => c.User)
                    .Where(c => c.UserId == userId && c.ProductId == productId)
                    .FirstOrDefaultAsync();

                if (existingCartItem == null)
                {
                    return NotFound("CartItem not found.");
                }

                if (existingCartItem.Quantity > quantity)
                {
                    existingCartItem.Quantity -= quantity;
                }
                else
                {
                    _dbContext.Carts.Remove(existingCartItem);
                }

                await _dbContext.SaveChangesAsync();

                return Ok("Product removed from the cart successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("BuyNow")]
        public async Task<IActionResult> BuyNow([FromBody] Cart request)
        {
            try
            {
                if (request == null || request.UserId <= 0 || request.ProductId <= 0)
                {
                    return BadRequest("Invalid request parameters.");
                }

                var product = await _dbContext.Products.FindAsync(request.ProductId);
                var client = await _dbContext.Clients.FindAsync(request.UserId);

                if (product == null || client == null)
                {
                    return NotFound("Product or client not found.");
                }

                var quantityToBuy = request.Quantity > 0 ? request.Quantity : 1;

                if (product.Quantity < quantityToBuy)
                {
                    return BadRequest("Not enough stock available.");
                }

                product.Quantity -= quantityToBuy;

                var newBill = new Bill
                {
                    Date = DateTime.UtcNow,
                    ClientId = request.UserId,
                    CreatedAt = DateTime.Now,
                };

                _dbContext.Bills.Add(newBill);
                await _dbContext.SaveChangesAsync();

                var newBillDetail = new Billdetail
                {
                    BillId = newBill.BillId,
                    ProductId = request.ProductId,
                    Quantity = quantityToBuy,
                    Price = quantityToBuy * product.Price 
                };

                _dbContext.Billdetails.Add(newBillDetail);
                await _dbContext.SaveChangesAsync();

                var tongtien = quantityToBuy * product.Price;
                var responseMessage = $"Product bought successfully. Bill Detail ID: {newBillDetail.BillId}. Total Cost: {tongtien}.";

                return Ok(responseMessage);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                Console.WriteLine($"Inner Exception: {ex.InnerException?.Message}");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("GetCart/{userId}")]
        public async Task<IActionResult> GetCart(int userId)
        {
            try
            {
                if (userId <= 0)
                {
                    return BadRequest("Invalid user ID.");
                }

                var cartItems = await _dbContext.Carts
                    .Include(c => c.Product)
                    .Where(c => c.UserId == userId)
                    .ToListAsync();

                if (cartItems == null || cartItems.Count == 0)
                {
                    return NotFound("Cart is empty.");
                }

                var cartDetails = cartItems.Select(cartItem => new
                {
                    ProductId = cartItem.ProductId,
                    ProductName = cartItem.Product.Name,
                    ProductImage = cartItem.Product.Image,
                    Quantity = cartItem.Quantity,
                    TotalAmount = cartItem.TotalAmount

                }).ToList();

                return Ok(cartDetails);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
//[HttpPut("UpdateCart/{userId}/{productId}/{quantity}")]
//public async Task<IActionResult> UpdateCart(int userId, int productId, int quantity)
//{
//    try
//    {
//        if (userId <= 0 || productId <= 0 || quantity <= 0)
//        {
//            return BadRequest("Invalid request parameters.");
//        }

//        var existingCartItem = await _dbContext.Carts
//            .Include(c => c.Product)
//            .Include(c => c.User)
//            .Where(c => c.UserId == userId && c.ProductId == productId)
//            .FirstOrDefaultAsync();

//        if (existingCartItem == null)
//        {
//            return NotFound("CartItem not found.");
//        }

//        if (quantity > existingCartItem.Product.Quantity)
//        {
//            return BadRequest("Requested quantity exceeds available stock.");
//        }

//        var originalQuantity = existingCartItem.Quantity;

//        existingCartItem.Product.Quantity -= quantity;

//        existingCartItem.Quantity = quantity;


//        var updatedTotalAmount = existingCartItem.TotalAmount;
//        await _dbContext.SaveChangesAsync();

//        return Ok($"Cart updated successfully. Updated Total Amount: {updatedTotalAmount}");

//    }
//    catch (Exception ex)
//    {
//        return StatusCode(500, $"Internal server error: {ex.Message}");
//    }
//}