using Razorpay.Api;
using System;
using System.Collections.Generic;
using WebApplication2.Models;

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
        RazorpayClient client = new RazorpayClient(_key, _secret);

        Dictionary<string, string> attributes = new Dictionary<string, string>
        {
            { "razorpay_order_id", razorpayOrderId },
            { "razorpay_payment_id", razorpayPaymentId },
            { "razorpay_signature", razorpaySignature }
        };

        try
        {
            Utils.verifyPaymentSignature(attributes);
            return true; // Signature is verified
        }
        catch (Exception ex)
        {
            // Log the exception or handle it appropriately
            return false; // Signature verification failed
        }
    }

    public string GetKey()
    {
        return _key;
    }

    public PaymentDetails FetchPaymentDetails(string paymentId)
    {
        if (string.IsNullOrEmpty(paymentId))
        {
            // Log or handle the case where the paymentId is null or empty
            throw new ArgumentNullException(nameof(paymentId), "Payment ID is null or empty");
        }
        RazorpayClient client = new RazorpayClient(_key, _secret);

        try
        {
            var payment = client.Payment.Fetch(paymentId);

            // Create a new PaymentDetails object and populate its properties
            var paymentDetails = new PaymentDetails
            {
                OrderId = payment["order_id"],
                PaymentId = payment["id"],
                Currency = payment["currency"],
                Amount = Convert.ToDecimal(payment["amount"]),
                CustomerName = payment["customer_name"],
                CustomerEmail = payment["customer_email"],
                Description = payment["description"],
                Status = payment["status"],
                PaymentMethod = payment["method"],
                CreatedAt = DateTime.UtcNow
            };

            return paymentDetails;
        }
        catch (Exception ex)
        {
            // Log the exception or handle it appropriately
            throw new Exception("Failed to fetch payment details from Razorpay API", ex);
        }
    }
}
