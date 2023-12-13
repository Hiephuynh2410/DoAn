using DoAn.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MimeKit;
using MailKit.Net.Smtp;

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
                product.Sold += 1;

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

                await SendBookingNotificationEmail(client.Email, newBillDetail, product, client);

                var tongtien = quantityToBuy * product.Price;
                var responseMessage = $"Product bought successfully. Bill Detail ID: {newBillDetail.BillId}. Total Cost: {tongtien}. Purchase Time: {newBill.CreatedAt}";

                return Ok(responseMessage);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                Console.WriteLine($"Inner Exception: {ex.InnerException?.Message}");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        private async Task SendBookingNotificationEmail(string clientEmail, Billdetail billDetail, Product product, Client client)
        {
            try
            {
                var message = new MimeMessage();

                message.From.Add(new MailboxAddress("PurchaseConfirm", "huynhhiepvan1998@gmail.com"));
                message.Subject = "Purchase Confirmation";

                message.Body = new TextPart("html")
                {
                    Text = $@"
                <!DOCTYPE html PUBLIC ""-//W3C//DTD XHTML 1.0 Transitional//EN"" ""http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd"">
                <html xmlns=""http://www.w3.org/1999/xhtml"">
                <head>
                    <meta http-equiv=""Content-Type"" content=""text/html; charset=utf-8"" />
                    <meta http-equiv=""X-UA-Compatible"" content=""IE=edge"" />
                    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                    <title>Purchase Confirmation</title>
                </head>
                <body style=""margin: 0;"">
                    <center style=""width: 100%; table-layout: fixed; background-color: #e5dec7;"" class=""wrapper"">
                        <table class=""main"" style=""border-spacing: 0; width: 100%; max-width: 500px; background-color: #ffffff; font-family: sans-serif; color: #4a4a4a; box-shadow: 0 0 25px rgba(0, 0, 0, .15);"" width=""100%"">
                            <tr>
                                <td style=""padding: 15px; background-color: #ffffff;"">
                                    <h2 style=""font-family: 'Roboto Condensed', sans-serif; font-size: 24px; text-align: center; color: #c89800; text-transform: uppercase;"">Xác nhận thanh toán</h2>
                                    <p style=""font-family: 'Roboto Condensed', sans-serif; font-size: 16px; color: #4a4a4a;"">
                                        Thân Mến {client.Name},
                                    </p>
                                    <p style=""font-family: 'Roboto Condensed', sans-serif; font-size: 16px; color: #4a4a4a;"">
                                        Cảm ơn bạn đã mua hàng của chúng tôi! Dưới đây là thông tin sản phảm bạn đã mua:
                                    </p>
                                    <table style=""border-spacing: 0; font-size: 16px; width: 100%;"">
                                        <tr>
                                            <td style=""padding: 10px;""><strong>Product:</strong></td>
                                            <td style=""padding: 10px;"">{product.Name}</td>
                                        </tr>
                                        <tr>
                                            <td style=""padding: 10px;""><strong>Quantity:</strong></td>
                                            <td style=""padding: 10px;"">{billDetail.Quantity}</td>
                                        </tr>
                                        <tr>
                                            <td style=""padding: 10px;""><strong>Total Price:</strong></td>
                                            <td style=""padding: 10px;"">{billDetail.Price}</td>
                                        </tr>
                                    </table>
                                    <p style=""font-family: 'Roboto Condensed', sans-serif; font-size: 16px; color: #4a4a4a;"">
                                        Cảm ơn bạn đã chọn dịch vụ của chúng tôi. Nếu bạn có câu hỏi nào, hãy cứ liên hệ với chúng tôi khi bạn cảm thấy thoải mái.
                                    </p>
                                    <p style=""font-family: 'Roboto Condensed', sans-serif; font-size: 16px; color: #4a4a4a;"">
                                        Trân trọng kính chào!
                                        <br />
                                        Booking
                                    </p>
                                </td>
                            </tr>
                        </table>
                    </center>
                </body>
                </html>
            "
                };

                using (var smtpClient = new SmtpClient())
                {
                    smtpClient.Connect("smtp.gmail.com", 587, false);
                    smtpClient.Authenticate("huynhhiepvan1998@gmail.com", "nmqt ljyf skbz xcrs");

                    message.To.Add(new MailboxAddress(clientEmail, clientEmail));

                    await smtpClient.SendAsync(message);

                    smtpClient.Disconnect(true);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error sending email: " + ex.Message);
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

        [HttpDelete("removeAll/{userId}")]
        public async Task<IActionResult> ClearCart(int userId)
        {
            try
            {
                if (userId <= 0)
                {
                    return BadRequest("Invalid user ID.");
                }

                var cartItems = await _dbContext.Carts
                    .Where(c => c.UserId == userId)
                    .ToListAsync();

                if (cartItems == null || cartItems.Count == 0)
                {
                    return NotFound("Cart is already empty.");
                }

                _dbContext.Carts.RemoveRange(cartItems);
                await _dbContext.SaveChangesAsync();

                return Ok("Cart cleared successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPut("UpdateCart/{userId}/{productId}")]
        public async Task<IActionResult> UpdateCartIncrease(int userId, int productId)
        {
            try
            {
                if (userId <= 0 || productId <= 0)
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

                existingCartItem.Quantity += 1;

                if (existingCartItem.Quantity > existingCartItem.Product.Quantity)
                {
                    return BadRequest("Không đủ sản phẩm trong giỏ hàng.");
                }

                var updatedTotalAmount = existingCartItem.TotalAmount;
                await _dbContext.SaveChangesAsync();

                return Ok($"Cart updated successfully. Updated Total Amount: {updatedTotalAmount}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPut("UpdateCartDecrease/{userId}/{productId}")]
        public async Task<IActionResult> UpdateCartDecrease(int userId, int productId)
        {
            try
            {
                if (userId <= 0 || productId <= 0)
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

                existingCartItem.Quantity -= 1;

                if (existingCartItem.Quantity < existingCartItem.Product.Quantity)
                {
                    return BadRequest("Không đủ sản phẩm trong giỏ hàng.");
                }

                var updatedTotalAmount = existingCartItem.TotalAmount;
                await _dbContext.SaveChangesAsync();

                return Ok($"Cart updated successfully. Updated Total Amount: {updatedTotalAmount}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("GetBestSellingProduct")]
        public async Task<IActionResult> GetBestSellingProduct()
        {
            try
            {
                var bestSellingProduct = await _dbContext.Products
                    .OrderByDescending(p => p.Sold)
                    .Take(10)
                    .ToListAsync();


                if (bestSellingProduct == null || bestSellingProduct.Count == 0)
                {
                    return NotFound("No products found.");
                }

                var results = bestSellingProduct.Select(product => new
                {
                    ProductId = product.ProductId,
                    Name = product.Name,
                    Description = product.Description,
                    Price = product.Price,
                    Quantity = product.Quantity,
                    ProductTypeId = product.ProductTypeId,
                    Image = product.Image,
                    ProviderId = product.ProviderId,
                    CreatedAt = product.CreatedAt,
                    CreatedBy = product.CreatedBy,
                    UpdatedAt = product.UpdatedAt,
                    UpdatedBy = product.UpdatedBy,
                    Sold = product.Sold
                });

                return Ok(results);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("GetAllBillDetails/{userId}")]
        public async Task<IActionResult> GetAllBillDetails(int userId)
        {
            if (userId <= 0)
            {
                return BadRequest("Invalid user ID.");
            }
            var billDetailsClient = await _dbContext.Billdetails
                .Include(x => x.Bill)
                .Include(x => x.Product)
                .Where(x => x.Bill.ClientId == userId).ToListAsync();

            if (billDetailsClient == null || billDetailsClient.Count == 0)
            {
                return NotFound("Khong tim thay billdetail.");
            }
            var results = billDetailsClient.Select(billDetail => new
            {
                BillId = billDetail.BillId,
                ProductId = billDetail.ProductId,
                Quantity = billDetail.Quantity,
                Price = billDetail.Price,
                ProductName = billDetail.Product.Name,

            });
            return Ok(results);
        }

        [HttpGet("GetBooking/{userId}")]
        public async Task<IActionResult> GetBooking(int userId)
        {
            try
            {
                if (userId <= 0)
                {
                    return BadRequest("Invalid user ID.");
                }

                var bookings = await _dbContext.Bookings
                    .Include(b => b.Bookingdetails)
                   .Where(b => b.ClientId == userId && b.Status == true)
                    .ToListAsync();

                if (bookings == null || bookings.Count == 0)
                {
                    return NotFound("No bookings found for the user.");
                }

                var bookingDetails = bookings.Select(booking => new
                {
                    BookingId = booking.BookingId,
                    Name = booking.Name,
                    Phone = booking.Phone,
                    DateTime = booking.DateTime,
                    Note = booking.Note,
                    Status = booking.Status,
                    ComboId = booking.ComboId,
                    CreatedAt = booking.CreatedAt,
                    BranchId = booking.BranchId,
                }).ToList();

                return Ok(bookingDetails);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

    }
}

