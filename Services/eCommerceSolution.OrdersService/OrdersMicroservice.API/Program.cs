using BusinessLogicLayer.HttpClients;
using eCommerce.OrdersMicroservice.BusinessLogicLayer;
using eCommerce.OrderMicroservice.DataAccessLayer;
using eCommerce.OrdersMicroservice.API.Middleware;
using eCommerce.OrdersMicroservice.BusinessLogicLayer.Policies;
using FluentValidation.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

//Add DAL and BAL services
builder.Services.AddDataAccessLayer(builder.Configuration);
builder.Services.AddBusinessLogicLayer(builder.Configuration);
builder.Services.AddControllers();

// Fluent Validation
builder.Services.AddFluentValidationAutoValidation();

//Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//Cors
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.WithOrigins("http://localhost:4200")
        .AllowAnyMethod()
        .AllowAnyHeader();
    });
});

builder.Services.AddTransient<IUsersMicroMicroservicePolicies, UsersMicroMicroservicePolicies>();
builder.Services.AddTransient<IProductsMicroMicroservicePolicies, ProductsMicroMicroservicePolicies>();
builder.Services.AddTransient<IPollyPolicies, PollyPolicies>();


builder.Services.AddHttpClient<UsersMicroserviceClient>(client =>
{
    client.BaseAddress = new Uri($"http://{builder.Configuration["UsersMicroserviceName"]}:{builder.Configuration["UsersMicroservicePort"]}");
})
.AddPolicyHandler(
   builder.Services.BuildServiceProvider().GetRequiredService<IUsersMicroMicroservicePolicies>().GetCombinedPolicy()
);

builder.Services.AddHttpClient<ProductsMicroserviceClient>(client =>
{
    client.BaseAddress = new Uri($"http://{builder.Configuration["ProductsMicroserviceName"]}:{builder.Configuration["ProductsMicroservicePort"]}");
})
.AddPolicyHandler(
   builder.Services.BuildServiceProvider().GetRequiredService<IProductsMicroMicroservicePolicies>().GetFallbackPolicy()
)
.AddPolicyHandler(
   builder.Services.BuildServiceProvider().GetRequiredService<IProductsMicroMicroservicePolicies>().GetBulkheadIsolationPolicy()
);

var app = builder.Build();

app.UseExceptionHandlingMiddleware();
app.UseRouting();

//Cors
app.UseCors();

//Swagger
app.UseSwagger();
app.UseSwaggerUI();

//Auth
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

//Endpoints
app.MapControllers();
app.Run();