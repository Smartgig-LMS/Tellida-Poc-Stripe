using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Stripe;
using System;
using System.IO;
using System.Threading.Tasks;

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
        public async Task<IActionResult> Index()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            const string endpointSecret = "whsec_UiEqwzpzaeUorfimWa0M8UvSmj8putQn";
            try
            {
                var stripeEvent = EventUtility.ParseEvent(json);
                var signatureHeader = Request.Headers["Stripe-Signature"];

                stripeEvent = EventUtility.ConstructEvent(json, signatureHeader, endpointSecret);

                if (stripeEvent.Type == Events.PaymentIntentSucceeded)
                {
                    var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
                    Console.WriteLine("A successful payment for {0} was made.", paymentIntent.Amount);
                    // Then define and call a method to handle the successful payment intent.
                    // handlePaymentIntentSucceeded(paymentIntent);
                }
                else if (stripeEvent.Type == Events.PaymentMethodAttached)
                {
                    var paymentMethod = stripeEvent.Data.Object as PaymentMethod;
                    // Then define and call a method to handle the successful attachment of a PaymentMethod.
                    // handlePaymentMethodAttached(paymentMethod);
                }
                else
                {
                    Console.WriteLine("Unhandled event type: {0}", stripeEvent.Type);
                }
                return Ok();
            }
            catch (StripeException e)
            {
                Console.WriteLine("Error: {0}", e.Message);
                return BadRequest();
            }
            catch (Exception e)
            {
                return StatusCode(500);
            }
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

public class StripeOptions
{
    public string SecretKey { get; set; }
    public string WebhookSecret { get; set; }
}



