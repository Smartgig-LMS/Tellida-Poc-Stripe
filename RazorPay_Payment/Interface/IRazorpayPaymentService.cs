using Razorpay.Api;

namespace RazorPay_Payment.Interface
{
    public interface IRazorpayPaymentService
    {
        // Method to create a new order with Razorpay
        public Order CreateOrder(decimal amount, string currency, string receipt);

        // Method to verify the payment signature
        bool VerifyPaymentSignature(string orderId, string paymentId, string signature);

        // Method to get the Razorpay key for client-side integration
        string GetKey();
    }
}
