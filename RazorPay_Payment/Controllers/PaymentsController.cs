using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Razorpay_Payment.Models;
using RazorPay_Payment.Models;
using RazorPay_Payment.Services;
using System.Security.Cryptography;
using System.Text;
namespace RazorPay_Payment.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentsController : ControllerBase
    {
        private readonly RazorpayPaymentService _razorpayPaymentService;
        private readonly MongoDBService _mongoDBService;

        public PaymentsController(IConfiguration configuration, MongoDBService mongoDBService)
        {
            var razorpayKey = configuration["Razorpay:Key"];
            var razorpaySecret = configuration["Razorpay:Secret"];

            _razorpayPaymentService = new RazorpayPaymentService(razorpayKey, razorpaySecret);
            _mongoDBService = mongoDBService;
        }

        [HttpPost("create-order")]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
        {
            try
            {
                var order = _razorpayPaymentService.CreateOrder(request.Amount, request.Currency, request.Receipt);
                var razorpayOrderId = order["id"].ToString();

                var orderDetails = new Orders
                {
                    RazorpayOrderId = razorpayOrderId,
                    Amount = request.Amount,
                    Currency = request.Currency,
                    Receipt = request.Receipt,
                    CustomerName = request.Name,
                    CustomerEmail = request.Email,
                    Description = request.Description,
                    CreatedAt = DateTime.UtcNow
                };

                await _mongoDBService.SaveOrderAsync(orderDetails);

                return Ok(new
                {
                    RazorpayOrderId = razorpayOrderId,
                    Amount = request.Amount,
                    Currency = request.Currency
 ,
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPost("verify-payment")]
        public IActionResult VerifyPayment([FromBody] VerifyPaymentRequest request)
        {
            bool isVerified = _razorpayPaymentService.VerifyPaymentSignature(request.RazorpayOrderId, request.RazorpayPaymentId, request.RazorpaySignature);

            if (isVerified)
            {
                return Ok(new { message = "Payment verified successfully." });
            }
            else
            {
                return BadRequest(new { message = "Payment verification failed." });
            }
        }
    }
}