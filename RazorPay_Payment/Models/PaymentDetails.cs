﻿using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace Razorpay_Payment.Models
{
    public class OrderResponse
    {
        public string RazorpayOrderId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public string Receipt { get; set; }
        public string CustomerName { get; set; }
        public string CustomerEmail { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
    }
    public class Orders
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } // The primary key, generated by MongoDB

        public string RazorpayOrderId { get; set; } // The order ID generated by Razorpay

        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public string Receipt { get; set; }
        public string CustomerName { get; set; }
        public string CustomerEmail { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class PaymentDetails
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string OrderId { get; set; } // Foreign key to Order
        public string PaymentId { get; set; }
        public string Currency { get; set; }
        public decimal Amount { get; set; }
        public string CustomerName { get; set; }
        public string CustomerEmail { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public string PaymentMethod { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateOrderRequest
    {
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public string Receipt { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Description { get; set; }
    }

    public class VerifyPaymentRequest
    {
        public string RazorpayOrderId { get; set; }
        public string RazorpayPaymentId { get; set; }
        public string RazorpaySignature { get; set; }
    }


}