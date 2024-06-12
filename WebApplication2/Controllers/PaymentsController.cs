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

public class PaymentsController : Controller
{
    private readonly RazorpayPaymentService _razorpayPaymentService;
    private readonly MongoDBService _mongoDBService;
    private readonly string _webhookSecret;

    public PaymentsController(IConfiguration configuration, MongoDBService? mongoDBService)
    {
        var razorpayKey = configuration["Razorpay:Key"];
        var razorpaySecret = configuration["Razorpay:Secret"];
        _webhookSecret = configuration["Razorpay:webhookSecret"];

        _razorpayPaymentService = new RazorpayPaymentService(razorpayKey, razorpaySecret);
        _mongoDBService = mongoDBService;
    }

    [HttpGet]
    public IActionResult CreateOrder()
    {
        return View();
    }
//create method for order
    [HttpPost]
    public async Task<IActionResult> CreateOrder(decimal amount, string currency, string receipt, string name, string email, string description)
    {
        try
        {
            var order = _razorpayPaymentService.CreateOrder(amount, currency, receipt);
            var razorpayOrderId = order["id"].ToString();

            // Save order details to MongoDB
            var orderDetails = new Orders
            {
                RazorpayOrderId = razorpayOrderId, // Assign the Razorpay order ID
                Amount = amount,
                Currency = currency,
                Receipt = receipt,
                CustomerName = name,
                CustomerEmail = email,
                Description = description,
                CreatedAt = DateTime.UtcNow
            };

            await _mongoDBService.SaveOrderAsync(orderDetails);

            ViewBag.RazorpayOrderId = razorpayOrderId;
            ViewBag.Amount = amount * 100; // Amount in the smallest currency unit
            ViewBag.Currency = currency;
            ViewBag.RazorpayKey = _razorpayPaymentService.GetKey();
            ViewBag.Name = name;
            ViewBag.Email = email;
            ViewBag.Description = description;

            // Log the order details for debugging
            Console.WriteLine($"Order created successfully: {razorpayOrderId}");

            return View("PaymentPage");
        }
        catch (Exception ex)
        {
            // Log the exception
            Console.WriteLine($"Order creation failed: {ex.Message}");
            return View("Error");
        }
    }

    [HttpPost]


    [HttpPost]
    public IActionResult VerifyPayment(string razorpay_order_id, string razorpay_payment_id, string razorpay_signature)
    {
        bool isVerified = !string.IsNullOrEmpty(razorpay_signature) && _razorpayPaymentService.VerifyPaymentSignature(razorpay_order_id, razorpay_payment_id, razorpay_signature);

        if (isVerified)
        {
            ViewBag.RazorpayOrderId = razorpay_order_id;
            ViewBag.RazorpayPaymentId = razorpay_payment_id;
            return View("Success");
        }
        else
        {
            return View("Failure");
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
}
