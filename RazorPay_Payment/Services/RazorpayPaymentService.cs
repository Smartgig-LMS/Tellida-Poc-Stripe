using Microsoft.Extensions.Primitives;
using Razorpay.Api;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

public class RazorpayPaymentService
{
    private readonly string _key;
    private readonly string _secret;

    public RazorpayPaymentService(string key, string secret)
    {
        _key = key;
        _secret = secret;
    }

    public Order CreateOrder(decimal amount, string currency, string receipt)
    {
        RazorpayClient client = new RazorpayClient(_key, _secret);

        var options = new Dictionary<string, object>
        {
            { "amount", amount * 100 }, // Amount in the smallest currency unit
            { "currency", currency },
            { "receipt", receipt },
            { "payment_capture", 1 } // Auto capture
        };

        return client.Order.Create(options);
    }

    public bool VerifyPaymentSignature(string razorpayOrderId, string razorpayPaymentId, string razorpaySignature)
    {
        var attributes = new Dictionary<string, string>
        {
            { "razorpay_order_id", razorpayOrderId },
            { "razorpay_payment_id", razorpayPaymentId },
            { "razorpay_signature", razorpaySignature }
        };

        try
        {
           Utils.verifyPaymentSignature(attributes);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public bool VerifyWebhookSignature(string payload, string signature)
    {
        var secret = Encoding.UTF8.GetBytes(_secret);
        var payloadBytes = Encoding.UTF8.GetBytes(payload);

        using (var hmac = new HMACSHA256(secret))
        {
            var hash = hmac.ComputeHash(payloadBytes);
            var hashString = BitConverter.ToString(hash).Replace("-", "").ToLower();

            return hashString.Equals(signature);
        }
    }
}
