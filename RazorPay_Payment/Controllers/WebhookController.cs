using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson.IO;
using Razorpay_Payment.Models;
using RazorPay_Payment.Services;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using System.IO;
using RazorPay_Payment.Models;


namespace RazorPay_Payment.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WebhookController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly MongoDBService _mongoDBService;
        private readonly string _webhookSecret;

        public WebhookController(IConfiguration configuration, MongoDBService mongoDBService)
        {
            _configuration = configuration;
            _webhookSecret = configuration["Razorpay:webhookSecret"];
            _mongoDBService = mongoDBService;
        }
        [HttpPost("handlewebhook")]
        public async Task<IActionResult> HandleWebhook()
        {
            using (StreamReader reader = new StreamReader(Request.Body, Encoding.UTF8))
            {
                var json = await reader.ReadToEndAsync();
                var receivedSignature = Request.Headers["X-Razorpay-Signature"].FirstOrDefault();
                var webhookPayload = Newtonsoft.Json.JsonConvert.DeserializeObject<RazorpayWebhookPayload>(json);

                if (webhookPayload != null && webhookPayload.@event.Contains("payment"))
                {
                    var paymentEntity = webhookPayload.payload.payment.entity;

                    var paymentDetails = new PaymentDetails
                    {
                        OrderId = paymentEntity.order_id,
                        PaymentId = paymentEntity.id,
                        Currency = paymentEntity.currency,
                        Amount = paymentEntity.amount / 100M, // Convert to major currency unit
                        CustomerName = paymentEntity.card?.name ?? paymentEntity.contact,
                        CustomerEmail = paymentEntity.email,
                        Description = paymentEntity.description,
                        Status = paymentEntity.status,
                        PaymentMethod = paymentEntity.method,
                        CreatedAt = DateTime.UtcNow
                    };

                    await _mongoDBService.SavePaymentDetailsAsync(paymentDetails);

                }
                else
                {
                    // Log if payload or event type is null
                    Console.WriteLine("Webhook payload or event type is null or does not contain payment.");
                }
            }

            return Ok();
        }

    }

}

