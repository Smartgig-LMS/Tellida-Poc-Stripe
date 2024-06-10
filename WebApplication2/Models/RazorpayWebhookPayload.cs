namespace WebApplication2.Models
{
    public class RazorpayWebhookPayload
    {
        public string @event { get; set; }
        public Payload payload { get; set; }

        public class Payload
        {
            public Payment payment { get; set; }

            public class Payment
            {
                public Entity entity { get; set; }

                public class Entity
                {
                    public string id { get; set; }
                    public string order_id { get; set; }
                    public string status { get; set; }
                    // Add other necessary fields as required
                }
            }
        }
    }
}
