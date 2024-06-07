using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace RazorPay_Payment.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RazorpayWebhookController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly RazorpayPaymentService _razorpayPaymentService;

        public RazorpayWebhookController(IConfiguration configuration)
        {
            _configuration = configuration;
            var razorpayKey = configuration["Razorpay:Key"];
            var razorpaySecret = configuration["Razorpay:Secret"];
            _razorpayPaymentService = new RazorpayPaymentService(razorpayKey, razorpaySecret);
        }

        [HttpPost]
        public async Task<IActionResult> Handle()
        {
            using (var reader = new StreamReader(Request.Body))
            {
                var body = await reader.ReadToEndAsync();
                var signature = Request.Headers["X-Razorpay-Signature"];

                // Verify the signature
                if (_razorpayPaymentService.VerifyWebhookSignature(body, signature))
                {
                    var jsonDoc = JsonDocument.Parse(body);
                    var eventType = jsonDoc.RootElement.GetProperty("event").GetString();

                    switch (eventType)
                    {
                        case "payment.captured":
                            // Handle payment captured event
                            break;
                        case "order.paid":
                            // Handle order paid event
                            break;
                        // Handle other events
                        default:
                            break;
                    }

                    return Ok();
                }
                else
                {
                    return BadRequest();
                }
            }
        }
    }
}
