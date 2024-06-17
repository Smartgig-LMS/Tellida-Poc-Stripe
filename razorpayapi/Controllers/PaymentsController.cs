using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using Newtonsoft.Json;
using Razorpay.Api;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using WebApplication2.Models;
using WebApplication2.Services;

[ApiController]
[Route("api/[controller]")]
public class PaymentsController : ControllerBase
{
    private readonly RazorpayPaymentService _razorpayPaymentService;
    private readonly MongoDBService _mongoDBService;
    private readonly string _webhookSecret;

    public PaymentsController(IConfiguration configuration, MongoDBService mongoDBService)
    {
        var razorpayKey = configuration["Razorpay:Key"];
        var razorpaySecret = configuration["Razorpay:Secret"];
        _webhookSecret = configuration["Razorpay:webhookSecret"];

        _razorpayPaymentService = new RazorpayPaymentService(razorpayKey, razorpaySecret);
        _mongoDBService = mongoDBService;
    }

    [HttpPost("CreateOrder")]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
    {
        try
        {
            var order = _razorpayPaymentService.CreateOrder(request.Amount, request.Currency, request.Receipt);
            var razorpayOrderId = order["id"].ToString();
            var status = order["status"].ToString(); // Assuming status is returned in the order response

            // Save order details to MongoDB
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

            // Create the response DTO
            var orderResponse = new OrderResponse
            {
                RazorpayOrderId = razorpayOrderId,
                Amount = request.Amount,
                Currency = request.Currency,
                Receipt = request.Receipt,
                CustomerName = request.Name,
                CustomerEmail = request.Email,
                Description = request.Description,
                Status = status
            };

            return Ok(orderResponse);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Error = "Order creation failed", Message = ex.Message });
        }
    }

[HttpPost("verify-payment")]
    public IActionResult VerifyPayment([FromBody] VerifyPaymentRequest request)
    {
        bool isVerified = !string.IsNullOrEmpty(request.RazorpaySignature) &&
                          _razorpayPaymentService.VerifyPaymentSignature(request.RazorpayOrderId, request.RazorpayPaymentId, request.RazorpaySignature);

        if (isVerified)
        {
            return Ok(new { Message = "Payment verified successfully", RazorpayOrderId = request.RazorpayOrderId, RazorpayPaymentId = request.RazorpayPaymentId });
        }
        else
        {
            return BadRequest(new { Error = "Payment verification failed" });
        }
    }

    [HttpPost]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> RazorpayWebhook([FromBody] RazorpayWebhookPayload payload)
    {
        try
        {
            // Log headers
            foreach (var header in Request.Headers)
            {
                Console.WriteLine($"{header.Key}: {header.Value}");
            }

            string signature = Request.Headers["X-Razorpay-Signature"];
            using (StreamReader reader = new StreamReader(Request.Body, Encoding.UTF8))
            {
                string body = await reader.ReadToEndAsync();
                Console.WriteLine("Request Body: " + body);

                if (VerifyWebhookSignature(body, signature))
                {
                    // Process the payload
                    switch (payload.@event)
                    {
                        case "payment.captured":
                        case "payment.failed":
                            var paymentEntity = payload.payload.payment.entity;

                            // Save payment details to MongoDB
                            var paymentDetails = new PaymentDetails
                            {
                                OrderId = paymentEntity.order_id,
                                PaymentId = paymentEntity.id,
                                Currency = paymentEntity.currency,
                                Amount = paymentEntity.amount / 100m, // Assuming amount is in the smallest currency unit
                                CustomerEmail = paymentEntity.email,
                                Description = paymentEntity.description,
                                Status = paymentEntity.status,
                                PaymentMethod = paymentEntity.method,
                                CreatedAt = DateTime.UtcNow
                            };

                            await _mongoDBService.SavePaymentDetailsAsync(paymentDetails);
                            Console.WriteLine($"Payment processed: {paymentEntity.id} with status {paymentEntity.status}");

                            break;

                        default:
                            Console.WriteLine("Unhandled event: " + payload.@event);
                            break;
                    }
                    return Ok();
                }
                else
                {
                    Console.WriteLine("Invalid signature");
                    return BadRequest("Invalid signature");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Webhook processing failed: {ex.Message}");
            return StatusCode(500, "Internal server error");
        }
    }


    private bool VerifyWebhookSignature(string payload, string signature)
    {
        using (var hmacsha256 = new HMACSHA256(Encoding.UTF8.GetBytes(_webhookSecret)))
        {
            byte[] hashmessage = hmacsha256.ComputeHash(Encoding.UTF8.GetBytes(payload));
            string expectedSignature = BitConverter.ToString(hashmessage).Replace("-", "").ToLower();

            return expectedSignature.Equals(signature);
        }
    }

    public class CreateOrderRequest
    {
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public string Receipt { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Description { get; set; }
    }
    public class OrderResponse
    {
        public string RazorpayOrderId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public string Receipt { get; set; }
        public string CustomerName { get; set; }
        public string CustomerEmail { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
    }

    public class VerifyPaymentRequest
    {
        public string RazorpayOrderId { get; set; }
        public string RazorpayPaymentId { get; set; }
        public string RazorpaySignature { get; set; }
    }
}
