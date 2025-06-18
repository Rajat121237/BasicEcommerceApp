using eCommerce.Infrastructure;
using eCommerce.API.Middlewares;
using eCommerce.Core;
using System.Text.Json.Serialization;
using eCommerce.Core.Mappers;
using FluentValidation.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

//Add Infrastructure & Core Services
builder.Services.AddInfrastructure();
builder.Services.AddCore();

//Add Controllers to the service collection
builder.Services.AddControllers().AddJsonOptions( options =>
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

//Add automapper to the service collection
builder.Services.AddAutoMapper(typeof(ApplicationUserMappingProfile).Assembly);

//Add fluentvalidation to the services collection
builder.Services.AddFluentValidationAutoValidation();

//Add API explorer services
builder.Services.AddEndpointsApiExplorer();

//Add swagger generation services to create swagger specification
builder.Services.AddSwaggerGen();

//Add CORS services
builder.Services.AddCors(option =>
{
    option.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:4200").AllowAnyMethod().AllowAnyHeader();
    });
});

//Build the web application
var app = builder.Build();

//Add Exception Handling Middleware
app.UseExceptionHandlingMiddleware();
//Routing 
app.UseRouting();
//Add endpoints that can serve the swagger.json
app.UseSwagger();
//Add swagger UI
app.UseSwaggerUI();
app.UseCors();

//Auth
app.UseAuthentication();
app.UseAuthorization();

//Controller Routes
app.MapControllers();

app.Run();
