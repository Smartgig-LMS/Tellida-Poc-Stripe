using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

[ApiController]
[Route("api/[controller]")]
public class PaymentsController : ControllerBase
{
    private readonly RazorpayPaymentService _razorpayPaymentService;

    public PaymentsController(IConfiguration configuration)
    {
        var razorpayKey = configuration["Razorpay:Key"];
        var razorpaySecret = configuration["Razorpay:Secret"];
        _razorpayPaymentService = new RazorpayPaymentService(razorpayKey, razorpaySecret);
    }

    [HttpPost("create-order")]
    public IActionResult CreateOrder([FromBody] CreateOrderRequest request)
    {
        var order = _razorpayPaymentService.CreateOrder(request.Amount, request.Currency, request.Receipt);
        return Ok(new
        {
            OrderId = order["id"],
            Amount = order["amount"],
            Currency = order["currency"]
        });
    }

    [HttpPost("verify-payment")]
    public IActionResult VerifyPayment([FromBody] VerifyPaymentRequest request)
    {
        bool isVerified = _razorpayPaymentService.VerifyPaymentSignature(request.RazorpayOrderId, request.RazorpayPaymentId, request.RazorpaySignature);
        if (isVerified)
        {
            return Ok(new { Message = "Payment verified successfully" });
        }
        else
        {
            return BadRequest(new { Message = "Payment verification failed" });
        }
    }
}

public class CreateOrderRequest
{
    public decimal Amount { get; set; }
    public string Currency { get; set; }
    public string Receipt { get; set; }
}

public class VerifyPaymentRequest
{
    public string RazorpayOrderId { get; set; }
    public string RazorpayPaymentId { get; set; }
    public string RazorpaySignature { get; set; }
}
