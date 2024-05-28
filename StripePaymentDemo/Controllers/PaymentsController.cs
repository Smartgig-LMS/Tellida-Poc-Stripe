using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Stripe;

namespace StripePaymentDemo.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentsController : ControllerBase
    {
        private readonly StripeOptions _stripeOptions;

        public PaymentsController(IOptions<StripeOptions> stripeOptions)
        {
            _stripeOptions = stripeOptions.Value;
            StripeConfiguration.ApiKey = _stripeOptions.SecretKey;
        }

        [HttpPost("create-payment-intent")]
        public IActionResult CreatePaymentIntent([FromBody] CreatePaymentIntentRequest request)
        {
            var options = new PaymentIntentCreateOptions
            {
                Amount = request.Amount,
                Currency = "usd",
            };

            var service = new PaymentIntentService();
            var paymentIntent = service.Create(options);

            return Ok(new { clientSecret = paymentIntent.ClientSecret });
        }

        [HttpPost("webhook")]
        public async Task<IActionResult> StripeWebhook()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            try
            {
                var stripeEvent = EventUtility.ConstructEvent(
                    json,
                    Request.Headers["Stripe-Signature"],
                    _stripeOptions.WebhookSecret
                );

                if (stripeEvent.Type == Events.PaymentIntentSucceeded)
                {
                    var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
                    // Handle successful payment intent
                }
                else if (stripeEvent.Type == Events.PaymentIntentPaymentFailed)
                {
                    var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
                    // Handle failed payment intent
                }

                return Ok();
            }
            catch (StripeException ex)
            {
                return BadRequest();
            }
        }
    }

    public class CreatePaymentIntentRequest
    {
        public long Amount { get; set; }
    }
}
