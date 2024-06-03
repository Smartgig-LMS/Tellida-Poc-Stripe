using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Stripe;

namespace StripePaymentDemo
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.Configure<StripeOptions>(options =>
            {
                options.SecretKey = "sk_test_51P9lfCSHxpNSNK1o2pmKxMlKWxYzuM1z1Pckwi93tjntUAHAyNEQSOUNYuv72WZqiFbLJTaHStdkLhtFVc8Km2mz00O48gfVv7";
                options.WebhookSecret = "we_1PLMNsSHxpNSNK1o8grTquiy";
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            StripeConfiguration.ApiKey = "sk_test_51P9lfCSHxpNSNK1o2pmKxMlKWxYzuM1z1Pckwi93tjntUAHAyNEQSOUNYuv72WZqiFbLJTaHStdkLhtFVc8Km2mz00O48gfVv7";
        }
    }
}
