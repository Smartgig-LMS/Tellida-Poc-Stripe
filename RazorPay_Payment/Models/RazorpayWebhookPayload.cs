namespace RazorPay_Payment.Models
{
    public class RazorpayWebhookPayload
    {
        public string account_id { get; set; }
        public List<string> contains { get; set; }
        public long created_at { get; set; }
        public string entity { get; set; }
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
            public AcquirerData acquirer_data { get; set; }
            public int amount { get; set; }
            public int amount_refunded { get; set; }
            public int amount_transferred { get; set; }
            public string bank { get; set; }
            public bool captured { get; set; }
            public Card card { get; set; }
            public string card_id { get; set; }
            public string contact { get; set; }
            public long created_at { get; set; }
            public string currency { get; set; }
            public string description { get; set; }
            public string email { get; set; }
            public string id { get; set; }
            public bool international { get; set; }
            public string invoice_id { get; set; }
            public string method { get; set; }
            public List<string> notes { get; set; }
            public string order_id { get; set; }
            public string refund_status { get; set; }
            public string status { get; set; }
            public string tax { get; set; }
            public string token_id { get; set; }
            public string vpa { get; set; }
            public string wallet { get; set; }
            public string error_code { get; set; }
            public string error_description { get; set; }
            public string error_reason { get; set; }
            public string error_source { get; set; }
            public string error_step { get; set; }
            public string fee { get; set; }
        }

        public class AcquirerData
        {
            public string auth_code { get; set; }
            public string rrn { get; set; }
        }

        public class Card
        {
            public bool emi { get; set; }
            public string entity { get; set; }
            public string id { get; set; }
            public string iin { get; set; }
            public bool international { get; set; }
            public string issuer { get; set; }
            public string last4 { get; set; }
            public string name { get; set; }
            public string network { get; set; }
            public string sub_type { get; set; }
            public string type { get; set; }
        }
    }
}
