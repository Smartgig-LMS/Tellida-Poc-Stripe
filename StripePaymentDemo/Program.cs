using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace StripePaymentDemo
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .UseUrls("http://*:5000"); // Ensure it listens on port 5000
                
    }
}
