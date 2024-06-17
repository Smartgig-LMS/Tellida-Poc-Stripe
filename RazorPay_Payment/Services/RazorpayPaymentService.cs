using Razorpay.Api;
using System;
using System.Collections.Generic;
using Razorpay_Payment.Models;
using RazorPay_Payment.Interface;

public class RazorpayPaymentService : IRazorpayPaymentService
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

}