using FluentValidation;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using WebAPI.Data;
using WebAPI.Endpoints;
using WebAPI.Extensions;
using WebAPI.Security.Enums;
using WebAPI.Services.Abstract;
using WebAPI.Services.Concrete;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http.Json;
using AspNetCoreRateLimit;
using WebAPI.Security;
using FootballAPI.Security;

var builder = WebApplication.CreateBuilder(args);
// Load secrets.json for sensitive data
builder.Configuration.AddJsonFile("secrets.json", optional: true, reloadOnChange: true);

// CORS
var allowedOrigins = new[] { "http://localhost:4200", "http://localhost:8100" };
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontendClients", policy =>
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod());
});

// Dependency Injection
builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddSingleton<ITokenHelper, JwtHelper>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICommentService, CommentService>();
builder.Services.AddScoped<IAddressService, AddressService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.RegisterSwagger();

// Auth
builder.Services.AddScoped<LogService>();
var tokenOptions = builder.Configuration.GetSection("TokenOptions").Get<TokenOptions>();
if (tokenOptions is not null)
    builder.Services.RegisterAuth(tokenOptions);

// Database
builder.Services.AddDbContext<ApplicationDBContext>(opt =>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("PostgresSQL")));

// JSON
builder.Services.Configure<JsonOptions>(options =>
    options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);

// Authorization
builder.Services.AddAuthorization();
builder.Services.AddAuthorizationBuilder()
    .AddPolicy("admin", policyBuilder => policyBuilder.RequireRole(Role.Admin.ToString()))
    .AddPolicy("seller", policyBuilder => policyBuilder.RequireRole(Role.Seller.ToString()))
    .AddPolicy("multi", policyBuilder => policyBuilder.RequireAssertion(context =>
        context.User.IsInRole(Role.Admin.ToString()) ||
        context.User.IsInRole(Role.Seller.ToString())));

// Rate limiting
builder.Services.AddMemoryCache();
builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
builder.Services.AddInMemoryRateLimiting();

var app = builder.Build();

// Global Exception Handling
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync("{\"error\":\"An unexpected error occurred.\"}");
    });
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowFrontendClients");
app.UseAuthentication();
app.UseAuthorization();

// Endpoints
// Exception logging middleware
app.UseMiddleware<WebAPI.Middleware.ExceptionLoggingMiddleware>();
app.RegisterAuthEndpoints();
app.RegisterProductEndpoints();
app.RegisterCommentEndpoints();
app.RegisterAddressEndpoints();
app.RegisterOrderEndpoints();
app.RegisterLogEndpoints();

app.UseStaticFiles();
app.UseIpRateLimiting();
app.Run();