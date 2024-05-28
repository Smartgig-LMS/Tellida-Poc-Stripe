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
            try
            {
                // Create payment intent with provided amount, currency, description, and customer details
                var options = new PaymentIntentCreateOptions
                {
                    Amount = request.Amount,
                    Currency = "usd",
                    Description = request.Description,
                    Shipping = new ChargeShippingOptions
                    {
                        Name = request.CustomerName,
                        Address = new AddressOptions
                        {
                            Line1 = request.CustomerAddressLine1,
                            City = request.CustomerCity,
                            PostalCode = request.CustomerPostalCode,
                            Country = request.CustomerCountry,
                        }
                    }
                };

                var service = new PaymentIntentService();
                var paymentIntent = service.Create(options);

                // Return the client secret to the frontend
                return Ok(new { clientSecret = paymentIntent.ClientSecret });
            }
            catch (StripeException e)
            {
                // Handle any Stripe errors
                return BadRequest(new { error = e.StripeError.Message });
            }
            catch (Exception ex)
            {
                // Handle any other exceptions
                return StatusCode(500, new { error = "An error occurred while processing the payment." });
            }
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
        public string Description { get; set; } // Description of the goods or services
        public string CustomerName { get; set; } // Customer name
        public string CustomerAddressLine1 { get; set; } // Customer address line 1
        public string CustomerCity { get; set; } // Customer city
        public string CustomerPostalCode { get; set; } // Customer postal code
        public string CustomerCountry { get; set; } // Customer country
    }
}
