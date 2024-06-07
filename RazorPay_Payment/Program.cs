using Microsoft.OpenApi.Models;
using WebApplication3.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.Configure<RazorpaySettings>(builder.Configuration.GetSection("Razorpay"));
builder.Services.AddScoped<RazorpayPaymentService>(provider =>
{
    var config = provider.GetRequiredService<IConfiguration>();
    var key = config["Razorpay:Key"];
    var secret = config["Razorpay:Secret"];
    return new RazorpayPaymentService(key, secret);
});

// Add Swagger generator
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Your API", Version = "v1" });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();

// Enable middleware to serve generated Swagger as a JSON endpoint.
app.UseSwagger();

// Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.), specifying the Swagger JSON endpoint.
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Your API V1");
    c.RoutePrefix = "swagger";
});

app.MapControllers();
app.Run();
