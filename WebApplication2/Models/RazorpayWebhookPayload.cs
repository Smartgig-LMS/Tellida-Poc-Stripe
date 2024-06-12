namespace WebApplication2.Models
{
    public class RazorpayWebhookPayload
    {
        public string @event { get; set; }
        public Payload payload { get; set; }

        public class Payload
        {
            public Payment payment { get; set; }
        }

        public class Payment
        {
            public Entity entity { get; set; }
        }

        public class Entity
        {
            public string id { get; set; }
            public string order_id { get; set; }
            public string status { get; set; }
            public string method { get; set; }
            public decimal amount { get; set; }
            public string currency { get; set; }
            public string email { get; set; }
            public string description { get; set; }
        }
    }
}
