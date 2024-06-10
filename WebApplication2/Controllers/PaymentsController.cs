using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Razorpay.Api;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using WebApplication2.Models;

public class PaymentsController : Controller
{
    private readonly RazorpayPaymentService _razorpayPaymentService;

    public PaymentsController(IConfiguration configuration)
    {
        var razorpayKey = configuration["Razorpay:Key"];
        var razorpaySecret = configuration["Razorpay:Secret"];
        _razorpayPaymentService = new RazorpayPaymentService(razorpayKey, razorpaySecret);
    }

    [HttpGet]
    public IActionResult CreateOrder()
    {
        return View();
    }

    [HttpPost]
    public IActionResult CreateOrder(decimal amount, string currency, string receipt)
    {
        try
        {
            var order = _razorpayPaymentService.CreateOrder(amount, currency, receipt);
            ViewBag.OrderId = order["id"];
            ViewBag.Amount = order["amount"];
            ViewBag.Currency = order["currency"];
            ViewBag.RazorpayKey = _razorpayPaymentService.GetKey();

            // Log the order details for debugging
            Console.WriteLine($"Order created successfully: {order["id"]}");

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
    public IActionResult VerifyPayment(string razorpay_order_id, string razorpay_payment_id, string razorpay_signature)
    {
        bool isVerified = _razorpayPaymentService.VerifyPaymentSignature(razorpay_order_id, razorpay_payment_id, razorpay_signature);
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
    public IActionResult RazorpayWebhook([FromBody] RazorpayWebhookPayload payload)
    {
        try
        {
            if (VerifyWebhookSignature(Request.Headers, payload))
            {
                // Process the payload
                switch (payload.@event)
                {
                    case "payment.captured":
                        // Handle payment captured
                        Console.WriteLine("Payment captured: " + payload.payload.payment.entity.id);
                        break;
                    case "payment.failed":
                        // Handle payment failed
                        Console.WriteLine("Payment failed: " + payload.payload.payment.entity.id);
                        break;
                    default:
                        Console.WriteLine("Unhandled event: " + payload.@event);
                        break;
                }
                return Ok();
            }
            else
            {
                return BadRequest("Invalid signature");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Webhook processing failed: {ex.Message}");
            return StatusCode(500, "Internal server error");
        }
    }

    private bool VerifyWebhookSignature(IHeaderDictionary headers, RazorpayWebhookPayload payload)
    {
        string webhookSecret = "A$123^HG*"; // Replace with your actual webhook secret from Razorpay
        string signature = headers["X-Razorpay-Signature"];
        string payloadJson = JsonConvert.SerializeObject(payload);

        using (var hmacsha256 = new HMACSHA256(Encoding.UTF8.GetBytes(webhookSecret)))
        {
            byte[] hashmessage = hmacsha256.ComputeHash(Encoding.UTF8.GetBytes(payloadJson));
            string expectedSignature = BitConverter.ToString(hashmessage).Replace("-", "").ToLower();

            return expectedSignature.Equals(signature);
        }
    }
}
