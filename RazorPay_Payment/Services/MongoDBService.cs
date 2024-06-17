﻿using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using System.Threading.Tasks;
using Razorpay_Payment.Models;

namespace RazorPay_Payment.Services
{
    public class MongoDBService
    {
        private readonly IMongoCollection<Orders> _ordersCollection;
        private readonly IMongoCollection<PaymentDetails> _paymentDetailsCollection;

        public MongoDBService(IConfiguration configuration)
        {
            var client = new MongoClient(configuration["MongoDB:ConnectionString"]);
            var database = client.GetDatabase(configuration["MongoDB:DatabaseName"]);
            _ordersCollection = database.GetCollection<Orders>(configuration["MongoDB:OrdersCollectionName"]);
            _paymentDetailsCollection = database.GetCollection<PaymentDetails>(configuration["MongoDB:PaymentDetailsCollectionName"]);
        }

        public async Task SaveOrderAsync(Orders order)
        {
            await _ordersCollection.InsertOneAsync(order);
        }

        public async Task SavePaymentDetailsAsync(PaymentDetails paymentDetails)
        {
            await _paymentDetailsCollection.InsertOneAsync(paymentDetails);
        }

    }
}